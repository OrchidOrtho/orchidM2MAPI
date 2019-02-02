using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using orchidM2MAPI.DataProviders;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentDataProvider _deptData;
        private readonly ILoggerFactory _logger;


        public DepartmentController(IDepartmentDataProvider deptData, ILoggerFactory logger)
        {
            _deptData = deptData;
            _logger = logger;

        }


        [HttpGet]
        [Route("{location}")]
        [ProducesResponseType(typeof(List<Department>), 200)]
        public async Task<List<Department>> GetAllDepartments(string location)
        {
            return await _deptData.GetAllDepartments(location);
        }


    }
}