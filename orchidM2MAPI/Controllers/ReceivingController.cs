using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [DescriptionAttribute("Receiving returns information related to the rcmast and child tables such as rcitem")]
    public class ReceivingController : ControllerBase
    {
        private readonly IReceivingDataProvider _receivingData;
        private readonly ILoggerFactory _logger;

        public ReceivingController(IReceivingDataProvider receivingData, ILoggerFactory logger)
        {
            _receivingData = receivingData;
            _logger = logger;
        }


        [HttpGet]
        [Route("{location}/{receivingNo}")]
        [ProducesResponseType(typeof(Receiving), 200)]
        public async Task<ActionResult<Receiving>> GetReceiving(string location, string receivingNo)
        {
            return await _receivingData.GetReceiving(location, receivingNo);
        }

        [HttpGet]
        [Route("{location}/poNo/{poNo}")]
        [ProducesResponseType(typeof(List<Receiving>), 200)]
        public async Task<ActionResult<List<Receiving>>> GetReceivedItems(string location, string poNo)
        {
            return await _receivingData.GetReceivedItems(location, poNo);
        }

        [HttpGet]
        [Route("{location}/{lastChecked}")]
        [ProducesResponseType(typeof(List<Receiving>), 200)]
        public async Task<List<Receiving>> GetReceivingSinceLastChecked(string location, string lastChecked)
        {
            try
            {
                DateTime passThis = DateTime.Parse(lastChecked);

                return await _receivingData.GetReceivingSinceLastChecked(location, passThis);
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

    }
}