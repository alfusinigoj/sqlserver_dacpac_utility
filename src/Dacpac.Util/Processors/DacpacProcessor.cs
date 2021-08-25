using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace PivotalServices.DacpacDeploy.Utility
{
    public class DacpacProcessor : IProcessor
    {
        private readonly ILogger<DacpacProcessor> logger;

        public DacpacProcessor(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DacpacProcessor>();
        }

        public void Execute(DacpacOptions options)
        {
            var connectionString = new SqlConnectionStringBuilder(options.Connection);

            var packageFileName = GetPackageFileName(options.Package);

            ReplaceTokensWithParameters(packageFileName, options.Parameters);

            switch (options.Command)
            {
                case "script":
                    Script(packageFileName, connectionString, options.Arguments);
                    break;
                case "deploy":
                    Deploy(packageFileName, connectionString, options.Arguments);
                    break;
                case "extract":
                    Extract(packageFileName, connectionString, options.Arguments);
                    break;
                default:
                    logger.LogError($"{options.Command} is an invalid command argument for --command or -c, please use --help or -h for more info!");
                    break;
            }
        }

        private void ReplaceTokensWithParameters(string packageFileName, string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
                return;

            var packageDir = $"{Path.Combine(GetBasePath(), Path.GetFileNameWithoutExtension(packageFileName))}";

            if (Directory.Exists(packageDir))
                Directory.Delete(packageDir, true);

            ZipFile.ExtractToDirectory(packageFileName, packageDir);

            var fileContentInLines = File.ReadAllLines($"{packageDir}\\postdeploy.sql");

            parameters = parameters.Trim('[', ']');
            var parametersList = parameters.Split('|');

            var checkConditionPosition = -1;
            var checkConditionLineReplace = string.Empty;

            for (int i = 0; i < fileContentInLines.Length; i++)
            {
                if (fileContentInLines[i].Contains("AND [CurrentVersion]"))
                {
                    checkConditionPosition = i;
                    checkConditionLineReplace = fileContentInLines[i].Replace("AND [CurrentVersion]", "AND 1=0 AND [CurrentVersion]");
                }

                foreach (var parameter in parametersList)
                {
                    var key = parameter.Split('=')[0];
                    var value = Helpers.GetFromEnvironmentVariableIfSo(parameter.Split('=')[1]);

                    var matches = new Regex(@"@@(.+?)@@").Matches(fileContentInLines[i]);

                    foreach (Match m in matches)
                    {
                        if (m.Groups[1].Value == key)
                        {
                            fileContentInLines[i] = fileContentInLines[i].Replace(m.Value, value);
                            fileContentInLines[checkConditionPosition] = checkConditionLineReplace;
                        }
                    }
                }
            }

            var fileContent = string.Join(Environment.NewLine, fileContentInLines);

            File.WriteAllText($"{packageDir}\\postdeploy.sql", fileContent);

            if (File.Exists(packageFileName))
                File.Delete(packageFileName);

            ZipFile.CreateFromDirectory(packageDir, packageFileName);
        }

        private void Script(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments)
        {
            logger.LogInformation($"Creating Deploy Script from Dacpac {packageFileName}..");

            var options = new PublishOptions
            {
                GenerateDeploymentScript = true,
                GenerateDeploymentReport = true,
                DeployOptions = new DacDeployOptions
                {
                    BlockOnPossibleDataLoss = true,
                    IncludeTransactionalScripts = true,
                }
            };

            LoadOptions(options.DeployOptions, arguments);

            using var dacpac = DacPackage.Load(packageFileName);
            var result = GetService(connectionString.ConnectionString).Script(dacpac, connectionString.InitialCatalog, options);
            File.WriteAllText(Path.Combine(GetBasePath(), $"Deploy_{connectionString.InitialCatalog}.sql"), result.DatabaseScript);
            File.WriteAllText(Path.Combine(GetBasePath(), $"Deploy_{connectionString.InitialCatalog}.xml"), result.DeploymentReport);
        }

        private void Deploy(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments)
        {
            logger.LogInformation($"Deploying Dacpac {packageFileName}..");

            var options = new DacDeployOptions
            {
                BlockOnPossibleDataLoss = true,
                IncludeTransactionalScripts = true,
            };

            LoadOptions(options, arguments);

            using var dacpac = DacPackage.Load(packageFileName);
            GetService(connectionString.ConnectionString).Deploy(dacpac, connectionString.InitialCatalog, true);
        }

        private void Extract(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments)
        {
            logger.LogInformation($"Extracting to Dacpac {packageFileName}..");

            var options = new DacExtractOptions
            {
                ExtractApplicationScopedObjectsOnly = true,
                ExtractReferencedServerScopedElements = false,
                VerifyExtraction = true,
                Storage = DacSchemaModelStorageType.Memory
            };

            LoadOptions(options, arguments);

            GetService(connectionString.ConnectionString).Extract(packageFileName, connectionString.InitialCatalog, connectionString.InitialCatalog, new Version(1, 0, 0), connectionString.InitialCatalog, null, options);
        }

        private string GetPackageFileName(string packageFileName)
        {
            if (Path.IsPathFullyQualified(packageFileName))
                return packageFileName;

            return Path.Combine(GetBasePath(), packageFileName);
        }

        private DacServices GetService(string connectionString)
        {
            var service = new DacServices(connectionString);

            service.Message += Service_Message;
            service.ProgressChanged += Service_Progress;

            return service;
        }

        private void LoadOptions<T>(T options, string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
                return;

            try
            {
                arguments = arguments.Trim('[', ']');
                var additionalArgumentsList = arguments.Split(',');

                foreach (var item in additionalArgumentsList)
                {
                    var key = item.Split('=')[0];
                    var value = item.Split('=')[1];

                    var prop = options.GetType().GetProperty(key);
                    if (prop != null)
                    {
                        var val = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(options, val);
                    }
                }
            }
            catch
            {
                throw new ArgumentException($"One or more of the arguments passed `{arguments}` is not in the correct format");
            }
        }

        private static string GetBasePath()
        {
            using var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }

        private void Service_Message(object sender, DacMessageEventArgs e)
        {
            logger.LogInformation($"Prefix: {e.Message.Prefix}, Number: { e.Message.Number}, MessageType: { e.Message.MessageType}, Message: { e.Message.Message}");
        }

        private void Service_Progress(object sender, DacProgressEventArgs e)
        {
            logger.LogInformation($"OperationId: {e.OperationId}, Status: { e.Status}, Message: { e.Message}");
        }
    }
}
