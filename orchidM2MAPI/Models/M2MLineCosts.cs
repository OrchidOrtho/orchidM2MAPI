using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class M2MLineCosts
    {

        public float MaterialActual { get; set; }
        public float ToolActual { get; set; }
        public float LaborActual { get; set; }
        public float OverheadActual { get; set; }
        public float SetupActual { get; set; }
        public float FixedActual { get; set; }
    }

    public class M2MRelCosts
    {

        public float MaterialCost { get; set; }
        public float ToolCost { get; set; }
        public float LaborCost { get; set; }
        public float OverheadCost { get; set; }
    }

    public class M2MResult
    {
        public int ReturnValue { get; set; }
    }
}
