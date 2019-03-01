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
    public class GaugeController : ControllerBase
    {
        private readonly IGaugeDataProvider _gaugeData;
        private readonly ILoggerFactory _logger;


        public GaugeController(IGaugeDataProvider gaugeData, ILoggerFactory logger)
        {
            _gaugeData = gaugeData;
            _logger = logger;

        }


        [HttpGet]
        [Route("Types/{location}")]
        [ProducesResponseType(typeof(List<GaugeType>), 200)]
        public async Task<List<GaugeType>> GetGaugeTypes(string location)
        {
            return await _gaugeData.GetGaugeTypes(location);
        }

        [HttpGet]
        [Route("{location}")]
        [ProducesResponseType(typeof(List<Gauge>), 200)]
        public async Task<List<Gauge>> GetGauges(string location)
        {
            try
            {
                return await _gaugeData.GetGauges(location);
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

    }
}