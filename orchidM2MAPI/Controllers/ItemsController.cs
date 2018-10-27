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
    public class ItemsController : ControllerBase
    {
        private readonly IItemDataProvider _itemData;

        public ItemsController(IItemDataProvider itemData)
        {
            _itemData = itemData;
        }


        [HttpGet]
        [Route("item/{location}/{partNo}/{rev}")]
        public async Task<ActionResult<Item>> GetItem(string location, string partNo, string rev)
        {
            return await _itemData.GetItem(location, partNo, rev);
        }
    }
}