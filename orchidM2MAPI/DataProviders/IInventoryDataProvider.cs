using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IInventoryDataProvider
    {
        Task<List<Inventory>> GetInventoryOnHand(string location, string partNo);

        Task<List<Inventory>> GetInventoryShipped(string location, string partNo);

        Task<List<Inventory>> GetInventoryJobs(string location, string partNo);


    }
}
