using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Demand
    {
        private string _itemNo;

        public string ItemNo {
            get { return _itemNo; }
            set { _itemNo = value; }
        }
        public string ItemRev { get; set; }
        public string OrchidSiteNumber { get; set; }
        public int OrchidYear { get; set; }
        public int OrchidPeriod { get; set; }
        public float TotalQty { get; set; }

    }
}
