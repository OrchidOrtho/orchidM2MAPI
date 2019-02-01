using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using orchidM2MAPI.DataProviders;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentDataProvider _docData;
        private readonly ILoggerFactory _logger;


        public DocumentController(IDocumentDataProvider docData, ILoggerFactory logger)
        {
            _docData = docData;
            _logger = logger;

        }


        [HttpGet]
        [Route("{location}")]
        [ProducesResponseType(typeof(List<Document>), 200)]
        public async Task<List<Document>> GetAllDocuments(string location)
        {
            return await _docData.GetAllDocuments(location);
        }

        [HttpGet]
        [Route("{location}/{lastChecked}")]
        [ProducesResponseType(typeof(List<Document>), 200)]
        public async Task<List<Document>> GetAllDocumentsSince(string location, string lastChecked)
        {
            try
            {
                DateTime passThis = DateTime.Parse(lastChecked);

                return await _docData.GetAllDocumentsSince(location, passThis);
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

    }
}