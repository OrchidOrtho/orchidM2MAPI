using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Shipping
    {

        [Key]
        public Int32 ShippingId { get; set; }
        public string ShipperNo { get; set; }
        public DateTime ShipDate { get; set; }
        public string CustomerNo { get; set; }
        public string ShipVia { get; set; }
        public string ShipType { get; set; }



        public List<ShippingItem> ShippingItems { get; set; }
    }
}
