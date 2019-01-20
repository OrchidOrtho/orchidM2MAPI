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
    public class PartController : ControllerBase
    {
        private readonly IPartDataProvider _partData;

        public PartController(IPartDataProvider partData)
        {
            _partData = partData;
        }


        [HttpGet]
        [Route("{location}/{partNo}/{rev}")]
        public async Task<ActionResult<Part>> GetPart(string location, string partNo, string rev)
        {
            return await _partData.GetPart(location, partNo, rev);
        }


        [HttpGet]
        [Route("{location}/{partNo}")]
        public async Task<ActionResult<List<Part>>> GetPart(string location, string partNo)
        {
            return await _partData.GetPart(location, partNo);
        }
    }
}