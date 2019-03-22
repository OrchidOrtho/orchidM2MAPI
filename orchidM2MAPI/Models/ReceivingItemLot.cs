using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ReceivingItemLot
    {
        public string LotNumber { get; set; }
        public float LotQuantity { get; set; }
        public DateTime ExpirationDate { get; set; }

    }
}
