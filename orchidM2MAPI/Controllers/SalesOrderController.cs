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
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderDataProvider _soData;

        public SalesOrderController(ISalesOrderDataProvider soData)
        {
            _soData = soData;
        }


        [HttpGet]
        [Route("{location}/{customerNo}/{customerPONo}")]
        [ProducesResponseType(typeof(SalesOrder), 200)]
        public async Task<ActionResult<SalesOrder>> GetSalesOrder(string location, string customerNo, string customerPONo)
        {
            return await _soData.GetSalesOrder(location, customerPONo, customerNo);
        }

        [HttpPost]
        [Route("{location}/item/")]
        [ProducesResponseType(typeof(SalesOrderItem), 200)]
        public async Task<ActionResult<SalesOrderItem>> UpdateSalesOrderDetail(string location, SalesOrderItem item)
        {
            return await _soData.UpdateSalesOrderDetail(location, item);
 
        }

    }
}