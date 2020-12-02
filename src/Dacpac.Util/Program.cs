using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace PivotalServices.Dacpac.Util
{
    class Program
    {
        static void Main(string[] args) => BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddSingleton<IProcessor, DacpacProcessor>();
                    });
                    host.ConfigureLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.AddFilter("Default", LogLevel.Information)
                               .AddFilter("Microsoft", LogLevel.Warning)
                               .AddFilter("System", LogLevel.Warning)
                               .AddConsole()
                               .AddDebug();
                    });
                })
            .UseDefaults()
            .Build()
            .Invoke(args);

        private static CommandLineBuilder BuildCommandLine()
        {
            var root = new RootCommand("Command line helper tool for executing SQL Server Data-Tier Application Framework package (dacpac). For more info, refer https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac?view=sql-dacfx-150"){
                 new Option<string>(new string[] { "--command", "-c" }, "Dacpac command (script, deploy and extract) "){ IsRequired = true },
                 new Option<string>(new string[] { "--package", "-p" }, "Dacpac package full path or relative to the this exe including dacpac filename. To pass the value as environment variable, set the value as env:<variable_name>"){ IsRequired = true },
                 new Option<string>(new string[] { "--connection", "-n" }, "Connection string of the target database. To pass the value as environment variable, set the value as env:<variable_name>"){ IsRequired = true },
                 new Option<string>(new string[] { "--arguments", "-a" }, "Array of arguments for commands; For script and deploy use DacDeployOptions(https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacdeployoptions?view=sql-dacfx-150); For extract use DacExtractOptions(https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.dac.dacextractoptions?view=sql-dacfx-150); E.g -a [BlockOnPossibleDataLoss=true,IncludeTransactionalScripts=false]"),
            };

            root.Handler = CommandHandler.Create<DacpacOptions, IHost>(Run);
            return new CommandLineBuilder(root);
        }

        private static void Run(DacpacOptions options, IHost host)
        {
            var serviceProvider = host.Services;
            var processor = serviceProvider.GetRequiredService<IProcessor>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

            try
            {
                var package = GetFromEnvironmentVariableIfSo(options.Package);
                var connection = GetFromEnvironmentVariableIfSo(options.Connection);
                var connectionStringBuilder = new SqlConnectionStringBuilder(options.Connection);

                switch (options.Command)
                {
                    case "script":
                        processor.Script(package, connectionStringBuilder, options.Arguments);
                        break;
                    case "deploy":
                        processor.Deploy(package, connectionStringBuilder, options.Arguments);
                        break;
                    case "extract":
                        processor.Extract(package, connectionStringBuilder, options.Arguments);
                        break;
                    default:
                        logger.LogError($"{options.Command} is an invalid command argument for --command or -c, please use --help or -h for more info!");
                        break;
                }

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                if (logger == null)
                    Console.Error.WriteLine(ex.ToString());
                else
                    logger.LogError(ex.ToString());

                Environment.ExitCode = 1;
            }
        }

        private static string GetFromEnvironmentVariableIfSo(string argumentValue)
        {
            if (argumentValue.StartsWith("env:"))
            {
                var variableName = argumentValue.Substring(4, argumentValue.Length - 4);
                argumentValue = Environment.GetEnvironmentVariable(variableName);
                if (String.IsNullOrWhiteSpace(argumentValue))
                    throw new ArgumentException($"{variableName} is not set!");
            }

            return argumentValue;
        }
    }
}
