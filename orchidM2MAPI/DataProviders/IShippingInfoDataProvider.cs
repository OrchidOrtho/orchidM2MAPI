using orchidM2MAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.DataProviders
{
    public interface IShippingInfoDataProvider
    {
        Task<ShippingAllInfo> GetShippingInfo(string location, string shipperNo);
    }
}
