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
    public class KPIController : ControllerBase
    {
        private readonly IKPIDataProvider _kpiData;
        private readonly ILoggerFactory _logger;


        public KPIController(IKPIDataProvider kpiData, ILoggerFactory logger)
        {
            _kpiData = kpiData;
            _logger = logger;

        }


        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(float), 200)]
        public async Task<float> GetKPIValue(KPI kpiItem)
        {
            return await _kpiData.GetKPIValue(kpiItem);
        }


    }
}