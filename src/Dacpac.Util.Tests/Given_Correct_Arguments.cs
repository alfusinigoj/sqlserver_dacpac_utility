using PivotalServices.DacpacDeploy.Utility;
using System;
using System.Data.SqlClient;
using Xunit;

namespace Dacpac.Util.Tests
{
    public class Given_Correct_Arguments
    {
        [Theory]
        [InlineData("Table1")]
        [InlineData("View1")]
        [InlineData("Procedure1")]
        [InlineData("Function1")]
        [InlineData("Function2")]
        [InlineData("DatabaseScalarFunction1")]
        public void Test_If_Deploys_Dacpac_Successfully(string objectName)
        {
            // Arrange
            var connectionString = "Data Source=localhost,1402;Initial Catalog=DbUtilsSample;Persist Security Info=True;User ID=sa;Password=IAmAlwaysKind!";
            var command = "-cdeploy";
            var dacpacFile = "-fDbUtilsSample.dacpac";
            var connection = $"-n{connectionString}";
            var arguments = "-a[BlockOnPossibleDataLoss=false]";

            var commandlineArguments = new[] { command, dacpacFile, connection, arguments };

            //Act
            Program.Main(commandlineArguments);

            //Assert
            var count = ExecuteScalar<int>(connectionString, $"select count(*) as IsExists from dbo.sysobjects where id = object_id('[dbo].[{objectName}]')");
            Assert.True(count > 0);
        }

        private T ExecuteScalar<T>(string conString, string sql)
        {
            using var con = new SqlConnection(conString);
            con.Open();
            using var cmd = new SqlCommand(sql, con);
            return (T)Convert.ChangeType(cmd.ExecuteScalar(), typeof(T));
        }
    }
}
