using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Job
    {
        [Key]
        public Int32 JobId { get; set; }

        public string JobNo { get; set; }
        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string PartDesc { get; set; }
        public string JobType { get; set; }
        public string JobComments { get; set; }
        public DateTime JobDueDate { get; set; }
        public float JobQuantity { get; set; }
        public string JobMemo { get; set; }
        public string JobStatus { get; set; }

        public List<JobRouteStep> Routing { get; set; }

    }
}
