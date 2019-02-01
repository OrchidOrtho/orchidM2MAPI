using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Document
    {
        [Key]
        public Int32 Id { get; set; }

        public string DocumentTitle { get; set; }
        public string DocumentType { get; set; }
        public string VersionNo { get; set; }
        public DateTime VersionDate { get; set; }
        public string DocumentNo { get; set; }
        public string ChangeDescription { get; set; }
        public Int32 VersionMasterId { get; set; }
        public string Status { get; set; }

    }
}
