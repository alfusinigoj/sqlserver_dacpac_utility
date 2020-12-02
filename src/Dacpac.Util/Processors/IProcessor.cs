using Microsoft.Data.SqlClient;

namespace PivotalServices.Dacpac.Util
{
    public interface IProcessor
    {
        void Script(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments);
        void Deploy(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments);
        void Extract(string packageFileName, SqlConnectionStringBuilder connectionString, string arguments);
    }
}
