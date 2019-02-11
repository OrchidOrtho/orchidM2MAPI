using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [DescriptionAttribute("Receiving returns information related to the rcmast and child tables such as rcitem")]
    public class ReceivingController : ControllerBase
    {
        private readonly IReceivingDataProvider _receivingData;

        public ReceivingController(IReceivingDataProvider receivingData)
        {
            _receivingData = receivingData;
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

    }
}