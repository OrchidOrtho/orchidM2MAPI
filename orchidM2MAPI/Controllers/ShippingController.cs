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
    public class ShippingController : ControllerBase
    {
        private readonly IShippingDataProvider _shippingData;

        public ShippingController(IShippingDataProvider shippingData)
        {
            _shippingData = shippingData;
        }


        [HttpGet]
        [Route("{location}/{shipperNo}")]
        public async Task<ActionResult<Shipping>> GetShipping(string location, string shipperNo)
        {
            //Sample Call = https://localhost:44398/api/shippinginfo/010/138392

            return await _shippingData.GetShipping(location, shipperNo);
        }
    }
}