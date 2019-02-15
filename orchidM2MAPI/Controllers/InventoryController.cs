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
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryDataProvider _invData;

        public InventoryController(IInventoryDataProvider invData)
        {
            _invData = invData;
        }


        [HttpGet]
        [Route("{location}/{partNo}/onhand/")]
        [ProducesResponseType(typeof(List<Inventory>), 200)]
        public async Task<ActionResult<List<Inventory>>> GetInventoryOnHand(string location, string partNo)
        {
            return await _invData.GetInventoryOnHand(location, partNo);
        }

        [HttpGet]
        [Route("{location}/{partNo}/shipped/")]
        [ProducesResponseType(typeof(List<Inventory>), 200)]
        public async Task<ActionResult<List<Inventory>>> GetInventoryShipped(string location, string partNo)
        {
            return await _invData.GetInventoryShipped(location, partNo);
        }

        [HttpGet]
        [Route("{location}/{partNo}/wip/")]
        [ProducesResponseType(typeof(List<Inventory>), 200)]
        public async Task<ActionResult<List<Inventory>>> GetInventoryJobs(string location, string partNo)
        {
            return await _invData.GetInventoryJobs(location, partNo);
        }

    }
}