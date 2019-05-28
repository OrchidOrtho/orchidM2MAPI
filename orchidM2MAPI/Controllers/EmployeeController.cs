using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using orchidM2MAPI.DataProviders;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeDataProvider _employeeData;

        public EmployeeController(IEmployeeDataProvider employeeData)
        {
            _employeeData = employeeData;
        }


        [HttpGet]
        [Route("{employeeno}")]
        [ProducesResponseType(typeof(Employee), 200)]
        public async Task<ActionResult<Employee>> GetEmployee(string employeeno)
        {
            return await _employeeData.GetEmployee(employeeno);
        }

        [HttpGet]
        [Route("All/{location}")]
        [ProducesResponseType(typeof(List<Employee>), 200)]
        public async Task<ActionResult<List<Employee>>> GetAllEmployees(string location)
        {
            return await _employeeData.GetAllEmployees(location);

        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(List<Employee>), 201)]
        public async Task<ActionResult<List<Employee>>> UpsertEmployees([FromBody] Employee[] emps)
        {
            return await _employeeData.UpsertEmployees(emps);

        }

    }
}