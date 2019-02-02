using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class EmployeeTitle
    {

        public string BusinessTitle { get; set; }
        public string SiteNo { get; set; }
        public string Title
        {
            get
            {
                return BusinessTitle.Replace("\r", "");
            }
        }
    }
}
