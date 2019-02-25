using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrderItem
    {
        [Key]
        public Int32 SalesOrderItemId { get; set; }

        public string SalesOrderItemNo { get; set; }
        public string InternalItemNo { get; set; }
        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string CustomerPartNo { get; set; }
        public string CustomerPartRev { get; set; }
        public DateTime DueDateToShip { get; set; }
        public float Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public string PartDesc { get; set; }
        public string PartGroup { get; set; }

    }
}
