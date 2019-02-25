using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrder
    {
        [Key]
        public Int32 SalesOrderId { get; set; }

        public string SalesOrderNo { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPONo { get; set; }
        public DateTime AcknowledgedDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        public List<SalesOrderItem> Items { get; set; }

    }
}
