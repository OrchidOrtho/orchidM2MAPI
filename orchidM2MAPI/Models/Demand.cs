using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Demand
    {

        public string ItemNo { get; set; }
        public string ItemRev { get; set; }
        public string OrchidSiteNumber { get; set; }
        public int OrchidYear { get; set; }
        public int OrchidPeriod { get; set; }
        public float TotalQty { get; set; }

    }
}
