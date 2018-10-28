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
    public class ShippingLotInfoController : ControllerBase
    {
        private readonly IShippingLotInfoDataProvider _shippingLotInfoData;

        public ShippingLotInfoController(IShippingLotInfoDataProvider shippingLotInfoData)
        {
            _shippingLotInfoData = shippingLotInfoData;
        }


        [HttpGet]
        [Route("shippinglotinfo/{location}/{shipperNo}")]
        public async Task<ActionResult<List<ShippingLotInfo>>> GetShippingLotInfo(string location, string shipperNo)
        {
            return await _shippingLotInfoData.GetShippingLotInfo(location, shipperNo);
        }
    }
}