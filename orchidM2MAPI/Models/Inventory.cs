using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Inventory
    {
        [Key]
        public Int32 InventoryId { get; set; }

        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string Facility { get; set; }
        public string Location { get; set; }
        public string BinNo { get; set; }
        public string LotNumber { get; set; }
        public float Quantity { get; set; }
        public string ShipperNo { get; set; }
        public DateTime ShipDate { get; set; }
        public string JobNo { get; set; }
        public DateTime JobDueDate { get; set; }
        public string JobStatus { get; set; }
        public float LotQuantity { get; set; }

    }
}
