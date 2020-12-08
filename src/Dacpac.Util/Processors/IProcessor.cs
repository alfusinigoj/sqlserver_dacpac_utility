using Microsoft.Data.SqlClient;

namespace PivotalServices.DacpacDeploy.Utility
{
    public interface IProcessor
    {
        void Execute(DacpacOptions options);
    }
}
