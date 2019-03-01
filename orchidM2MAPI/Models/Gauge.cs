using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Gauge
    {
        [Key]
        public Int32 GaugeId { get; set; }

        public string GaugeNo { get; set; }
        public string GaugeName { get; set; }
        public string GaugeSubcategory { get; set; }
        public string Status { get; set; }
        public Int32 GaugeSubcategoryId { get; set; }
        public string Site { get; set; }
    }
}
