using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IItemDataProvider
    {
        Task<Item> GetItem(string location, string partNo, string rev);

    }
}
