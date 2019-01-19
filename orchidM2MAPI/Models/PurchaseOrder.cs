using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class PurchaseOrder
    {
        public string fpono { get; set; }
        public string fstatus { get; set; }
        public string fvendno { get; set; }
        public string fcompany { get; set; }
        public DateTime forddate { get; set; }
        public string fpartno { get; set; }
        public string frev { get; set; }
        public decimal fordqty { get; set; }
        public string fvmeasure { get; set; }
        public string finspect { get; set; }
    }
}
