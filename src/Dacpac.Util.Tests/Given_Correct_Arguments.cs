using PivotalServices.DacpacDeploy.Utility;
using System;
using System.Data.SqlClient;
using Xunit;

namespace Dacpac.Util.Tests
{
    public class Given_Correct_Arguments
    {
        [Fact]
        public void Test_If_Deploys_Dacpac_Successfully()
        {
            // Arrange
            var connectionString = "Data Source = localhost,1402; Initial Catalog = DbUtilTests; Persist Security Info = True; User ID = sa; Password = IAmAlwaysKind!";
            var command = "-c deploy";
            var dacpacFile = "-f DbUtilsSample.dacpac";
            var connection = $"-n \"{connectionString}\"";
            var arguments = "-a [BlockOnPossibleDataLoss=false]";

            var commandlineArguments = new[] { command, dacpacFile, connection, arguments };

            //Act
            Program.Main(commandlineArguments);

            //Assert

            using var con = new SqlConnection(connectionString);
            con.Open();
            string sql = "SELECT name, collation_name FROM sys.databases";
            using var cmd = new SqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
            }
        }
    }
}
