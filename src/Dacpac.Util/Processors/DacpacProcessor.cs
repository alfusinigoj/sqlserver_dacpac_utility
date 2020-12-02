using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using System;
using System.IO;

namespace PivotalServices.Dacpac.Util
{
    public class DacpacProcessor : IProcessor
    {
        private readonly ILogger<DacpacProcessor> logger;

        public DacpacProcessor(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DacpacProcessor>();
        }

        public void Script(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments)
        {
            packageFileName = GetPackageFileName(packageFileName);

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

            using (var dacpac = DacPackage.Load(packageFileName))
            {
                var result = GetService(connectionString.ConnectionString).Script(dacpac, connectionString.InitialCatalog, options);
                File.WriteAllText(Path.Combine(GetBasePath(), $"Deploy_{connectionString.InitialCatalog}.sql"), result.DatabaseScript);
                File.WriteAllText(Path.Combine(GetBasePath(), $"Deploy_{connectionString.InitialCatalog}.xml"), result.DeploymentReport);
            }
        }

        public void Deploy(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments)
        {
            packageFileName = GetPackageFileName(packageFileName);

            logger.LogInformation($"Deploying Dacpac {packageFileName}..");

            var options = new DacDeployOptions
            {
                BlockOnPossibleDataLoss = true,
                IncludeTransactionalScripts = true,
            };

            LoadOptions(options, arguments);

            using (var dacpac = DacPackage.Load(packageFileName))
            {
                GetService(connectionString.ConnectionString).Deploy(dacpac, connectionString.InitialCatalog, true);
            }
        }

        public void Extract(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments)
        {
            packageFileName = GetPackageFileName(packageFileName);

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
