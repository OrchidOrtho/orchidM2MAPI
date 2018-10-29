using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ShippingAllInfo
    {
        public List<ShippingLotInfo> LotInfoList { get; set; }
        public List<ShippingInfo> ShippingInfoList { get; set; }
    }
}
