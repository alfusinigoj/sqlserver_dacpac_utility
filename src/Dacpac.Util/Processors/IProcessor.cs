using Microsoft.Data.SqlClient;

namespace Dacpac.CommandLine
{
    public interface IProcessor
    {
        void Execute(DacpacOptions options);
    }
}
