using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class KPI
    {
        [Key]
        public Int32 KPIId { get; set; }

        public string ConnectionString { get; set; }
        public string SQLStatement { get; set; }
    }
}
