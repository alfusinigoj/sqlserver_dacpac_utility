using Microsoft.Data.SqlClient;
using System;
using System.Text;

namespace Dacpac.CommandLine
{
    public class DacpacOptions
    {
        public DacpacOptions(string command, string package, string connection, string arguments, string parameters)
        {
            Command = Helpers.GetFromEnvironmentVariableIfSo(command);
            Package = Helpers.GetFromEnvironmentVariableIfSo(package);
            Connection = Helpers.GetFromEnvironmentVariableIfSo(connection);
            Arguments = Helpers.GetFromEnvironmentVariableIfSo(arguments);
            Parameters = Helpers.GetFromEnvironmentVariableIfSo(parameters);
        }

        public string Command { get; }
        public string Package { get; }
        public string Connection { get; }
        public string Arguments { get; }
        public string Parameters { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (!prop.Name.Contains("pass", StringComparison.OrdinalIgnoreCase))
                {
                    if (prop.Name.Contains("connection", StringComparison.OrdinalIgnoreCase))
                    {
                        var connectionString = new SqlConnectionStringBuilder(prop.GetValue(this).ToString());
                        builder.AppendLine($"{prop.Name}: {$"Connection: DataSource: {connectionString.DataSource}, Initial Catalog: {connectionString.InitialCatalog}, UserName: {connectionString.UserID}"}");
                    }
                    else
                        builder.AppendLine($"{prop.Name}: {prop.GetValue(this)}");
                }
            }
            return builder.ToString();
        }
    }
}
