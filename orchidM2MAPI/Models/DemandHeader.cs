using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class DemandHeader
    {
        [Key]
        public Int32 DemandHeaderId { get; set; }

        public List<Demand> DemandItems { get; set; }

    }
}
