using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IJobDataProvider
    {
        Task<Job> GetJob(string location, string jobNo);

    }
}
