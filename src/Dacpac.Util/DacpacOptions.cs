using Microsoft.Data.SqlClient;
using System;
using System.Text;

namespace PivotalServices.Dacpac.Util
{
    public class DacpacOptions
    {
        public DacpacOptions(string command, string package, string connection, string arguments)
        {
            Command = command;
            Package = package;
            Connection = connection;
            Arguments = arguments;
        }

        public string Command { get; }
        public string Package { get; }
        public string Connection { get; }
        public string Arguments { get; }

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
