using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IDocumentDataProvider
    {
        Task<List<Document>> GetAllDocuments(string location);

        Task<List<Document>> GetAllDocumentsSince(string location, DateTime lastChecked);

    }
}
