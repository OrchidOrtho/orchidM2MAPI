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
    public class POInfoController : ControllerBase
    {
        private readonly IPOInfoDataProvider _poInfoData;

        public POInfoController(IPOInfoDataProvider poInfoData)
        {
            _poInfoData = poInfoData;
        }


        [HttpGet]
        [Route("poinfo/{location}/{poNo}/{poItemReleaseNo}")]
        public async Task<ActionResult<POInfo>> GetPOInfo(string location, string poNo, string poItemReleaseNo)
        {

            //Sample Call = https://localhost:44398/api/poinfo/poinfo/010/056967/4

            return await _poInfoData.GetPOInfo(location, poNo, poItemReleaseNo);
        }
    }
}