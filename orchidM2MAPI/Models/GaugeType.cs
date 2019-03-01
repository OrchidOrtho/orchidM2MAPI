using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class GaugeType
    {
        [Key]
        public Int32 GaugeTypeId { get; set; }

        public string GaugeTypeName { get; set; }
        public string Status { get; set; }

    }
}
