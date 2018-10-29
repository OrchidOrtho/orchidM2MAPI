using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ShippingInfo
    {
        public string fshipno { get; set; }
        public DateTime fshipdate { get; set; }
        public string fpartno { get; set; }
        public string frev { get; set; }
        public string fshipqty { get; set; }
        public string fmeasure { get; set; }
        public string fclot { get; set; }
        public string fnlotqty { get; set; }
        public string SaleOrderNumber { get; set; }
        public string fcustpono { get; set; }
        public string fdescript { get; set; }
        public string fcustpart { get; set; }
        public string CustomerPOLineNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string SalesOrderLineNumber { get; set; }
    }
}
