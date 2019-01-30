using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ShippingItem
    {
        [Key]
        public Int32 ShippingItemId { get; set; }
        public string ShippingItemNo { get; set; }
        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string Facility { get; set; }
        public float ShipQuantity { get; set; }
        public string ShipUnitofMeasure { get; set; }
        public string SalesOrderNo { get; set; }
        public string SalesOrderReleaseNo { get; set; }
        public string SalesOrderReleaseLineI { get; set; }
        public string SalesOrderReleaseLineE { get; set; }
        public string CustomerPONo { get; set; }
        public string CustomerPOLineNo { get; set; }
        public string CustomerPartNo { get; set; }
        public string CustomerPartRev { get; set; }
        public string PartDesc { get; set; }

        public List<ShippingLot> ShippingItemLots { get; set; }

    }
}
