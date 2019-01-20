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
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderDataProvider _poData;

        public PurchaseOrderController(IPurchaseOrderDataProvider poData)
        {
            _poData = poData;
        }


        [HttpGet]
        [Route("{location}/{poNo}/{poItemReleaseNo}")]
        public async Task<ActionResult<PurchaseOrder>> GetPOInfo(string location, string poNo, string poItemReleaseNo)
        {

            //Sample Call = https://localhost:44398/api/purchaseorder/010/056967/4

            return await _poData.GetPurchaseOrder(location, poNo, poItemReleaseNo);
        }
    }
}