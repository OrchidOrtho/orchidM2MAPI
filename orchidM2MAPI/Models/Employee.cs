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
        public string EmployeeFullName { get; set; }


    }
}
