using Microsoft.Data.SqlClient;

namespace PivotalServices.Dacpac.Util
{
    public interface IProcessor
    {
        void Execute(DacpacOptions options);
    }
}
