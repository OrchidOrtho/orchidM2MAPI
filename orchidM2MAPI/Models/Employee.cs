using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class Employee
    {
        [Key]
        public Int32 EmployeeId { get; set; }

        public string EmployeeNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public string BusinessTitle { get; set; }
        public string SiteNo { get; set; }
        public string Shift { get; set; }
        public string Supervisor { get; set; }
        public string DeptName { get; set; }

        public string DepartmentName
        {
            get
            {
                if (DeptName == null)
                    return "";
                else
                    return DeptName.Trim().Replace("\r", "").Replace("\n", "");

            }
        }
        public string Title
        {
            get
            {
                return BusinessTitle.Replace("\r", "");
            }
        }

        public string FullName
        {
            get
            {
                return FirstName + ' ' + LastName;
            }
        }

    }
}
