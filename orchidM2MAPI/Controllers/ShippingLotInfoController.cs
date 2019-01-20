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
    public class ShippingLotInfoController : ControllerBase
    {
        private readonly IShippingLotInfoDataProvider _shippingLotInfoData;

        public ShippingLotInfoController(IShippingLotInfoDataProvider shippingLotInfoData)
        {
            _shippingLotInfoData = shippingLotInfoData;
        }


        [HttpGet]
        [Route("{location}/{shipperNo}")]
        public async Task<ActionResult<List<ShippingLotInfo>>> GetShippingLotInfo(string location, string shipperNo)
        {
            //Sample Call = https://localhost:44398/api/shippinglotinfo/010/138392

            return await _shippingLotInfoData.GetShippingLotInfo(location, shipperNo);
        }
    }
}