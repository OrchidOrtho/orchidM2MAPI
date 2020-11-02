using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IDemandDataProvider
    {
        Task<List<Demand>> GetDemand(string location, string partNos);

        Task<DemandHeader> GetDemandHeader(string location, DemandHeader dh);

    }
}
