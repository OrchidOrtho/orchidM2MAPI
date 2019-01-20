using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IReceivingDataProvider
    {
        Task<Receiving> GetReceiving(string location, string receivingNo);

    }
}
