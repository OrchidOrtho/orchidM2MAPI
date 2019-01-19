using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IPartDataProvider
    {
        Task<Part> GetPart(string location, string partNo, string rev);
        Task<List<Part>> GetPart(string location, string partNo);

    }
}
