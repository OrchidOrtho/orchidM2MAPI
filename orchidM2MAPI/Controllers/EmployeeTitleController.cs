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
    public class EmployeeTitleController : ControllerBase
    {
        private readonly IEmployeeTitleDataProvider _empTitleData;
        private readonly ILoggerFactory _logger;


        public EmployeeTitleController(IEmployeeTitleDataProvider empTitleData, ILoggerFactory logger)
        {
            _empTitleData = empTitleData;
            _logger = logger;

        }


        [HttpGet]
        [Route("{location}")]
        [ProducesResponseType(typeof(List<EmployeeTitle>), 200)]
        public async Task<List<EmployeeTitle>> GetAllDocuments(string location)
        {
            return await _empTitleData.GetAllTitles(location);
        }


    }
}