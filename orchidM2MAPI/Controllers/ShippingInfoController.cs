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
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingInfoController : ControllerBase
    {
        private readonly IShippingInfoDataProvider _shippingInfoData;

        public ShippingInfoController(IShippingInfoDataProvider shippingInfoData)
        {
            _shippingInfoData = shippingInfoData;
        }


        [HttpGet]
        [Route("{location}/{shipperNo}")]
        public async Task<ActionResult<ShippingAllInfo>> GetShippingLotInfo(string location, string shipperNo)
        {
            //Sample Call = https://localhost:44398/api/shippinginfo/010/138392

            return await _shippingInfoData.GetShippingInfo(location, shipperNo);
        }
    }
}