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
    public class JobController : ControllerBase
    {
        private readonly IJobDataProvider _jobData;

        public JobController(IJobDataProvider jobData)
        {
            _jobData = jobData;
        }


        [HttpGet]
        [Route("{location}/{jobNo}")]
        [ProducesResponseType(typeof(Job), 200)]
        public async Task<ActionResult<Job>> GetJob(string location, string jobNo)
        {
            return await _jobData.GetJob(location, jobNo);
        }

        [HttpGet]
        [Route("{location}/lots/{jobNo}")]
        [ProducesResponseType(typeof(Job), 200)]
        public async Task<ActionResult<Job>> GetJobLots(string location, string jobNo)
        {
            return await _jobData.GetJobLots(location, jobNo);
        }


    }
}