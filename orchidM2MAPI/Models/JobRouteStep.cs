using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class JobRouteStep
    {
        [Key]
        public Int32 JobRouteStepId { get; set; }

        public Int32 OperationNo { get; set; }
        public string WorkCenterNo { get; set; }
        public string WorkCenterName { get; set; }
        public float QuantityComplete { get; set; }
        public string OperationMemo { get; set; }
        public float EstSetupTime { get; set; }
        public float EstProductionTimePerUnit { get; set; }


    }
}
