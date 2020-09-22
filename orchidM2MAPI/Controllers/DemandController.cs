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
    public class DemandController : ControllerBase
    {
        private readonly IDemandDataProvider _demandData;

        public DemandController(IDemandDataProvider demandData)
        {
            _demandData = demandData;
        }


        [HttpGet]
        [Route("{location}/{partNos}")]
        public async Task<ActionResult<List<Demand>>> GetDemand(string location, string partNos)
        {
            return await _demandData.GetDemand(location, partNos);
        }
    }
}