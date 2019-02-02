using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Department
    {

        public string DeptName { get; set; }
        public string SiteNo { get; set; }
        public string DepartmentName
        {
            get
            {

                    return DeptName.Trim().Replace("\r", "").Replace("\n", "");

            }
        }
    }
}
