using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ShippingLot
    {
        [Key]
        public Int32 ShippingItemLotId { get; set; }
        public string LotNo { get; set; }
        public float LotQuantity { get; set; }
        public string LaserEtch { get; set; }

        public List<ShippingLotBOM> ShippingItemLotBOM { get; set; }

    }
}
