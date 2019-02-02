using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using orchidM2MAPI.Models;
using Dapper;
using Microsoft.Extensions.Logging;

namespace orchidM2MAPI.DataProviders
{
    public class DocumentDataProvider : IDocumentDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public DocumentDataProvider(IConfigurationRoot config, ILoggerFactory logger)
        {
            _config = config;
            _logger = logger;
        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:MQ1";
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);

        }

        public async Task<List<Document>> GetAllDocuments(string location)
        {
            try
            {
                string sQuery = "";

                sQuery = "SELECT        dbo.Documents_p.Id, dbo.Documents_p.DocumentTitle_f AS DocumentTitle, dbo.DocumentTypes_p.DocumentType_f AS DocumentType, dbo.Documents_p.VersionNumber AS VersionNo, dbo.Documents_p.VersionDate,                          dbo.Documents_p.DocumentNo_f AS DocumentNo, dbo.Documents_p.ChangeDescription_f AS ChangeDescription, dbo.Documents_p.VersionMasterId, dbo.__State.Name AS Status, dbo.Sites_p.SiteAbbreviation_f AS SiteNo FROM            dbo.Documents_p INNER JOIN                          dbo.Sites_p ON dbo.Documents_p.Site_f = dbo.Sites_p.Id INNER JOIN                          dbo.DocumentTypes_p ON dbo.Documents_p.DocumentType_f = dbo.DocumentTypes_p.Id INNER JOIN                          dbo.__State ON dbo.Documents_p.CurrentState = dbo.__State.Id WHERE (dbo.Documents_p.IsRemoved = 0) AND (dbo.Sites_p.SiteAbbreviation_f = @location)";


                using (var connection = Connection(location))
                {
                    var documentDictionary = new Dictionary<int, Document>();


                    var list = connection.Query<Document>(
                        sQuery,
                        param: new { location = location })
                    .ToList();

                    return list;

                }
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

        public async Task<List<Document>> GetAllDocumentsSince(string location, DateTime lastChecked)
        {
            try
            {
                string sQuery = "";

                sQuery = "SELECT        dbo.Documents_p.Id, dbo.Documents_p.DocumentTitle_f AS DocumentTitle, dbo.DocumentTypes_p.DocumentType_f AS DocumentType, dbo.Documents_p.VersionNumber AS VersionNo, dbo.Documents_p.VersionDate,                          dbo.Documents_p.DocumentNo_f AS DocumentNo, dbo.Documents_p.ChangeDescription_f AS ChangeDescription, dbo.Documents_p.VersionMasterId, dbo.__State.Name AS Status, dbo.Sites_p.SiteAbbreviation_f AS SiteNo FROM            dbo.Documents_p INNER JOIN                          dbo.Sites_p ON dbo.Documents_p.Site_f = dbo.Sites_p.Id INNER JOIN                          dbo.DocumentTypes_p ON dbo.Documents_p.DocumentType_f = dbo.DocumentTypes_p.Id INNER JOIN                          dbo.__State ON dbo.Documents_p.CurrentState = dbo.__State.Id WHERE (dbo.Documents_p.IsRemoved = 0) AND (dbo.Sites_p.SiteAbbreviation_f = @location) AND (dbo.Documents_p.LastModifiedOn >= @lastDateTime)";


                using (var connection = Connection(location))
                {
                    var documentDictionary = new Dictionary<int, Document>();


                    var list = connection.Query<Document>(
                        sQuery,
                        param: new { location = location, lastDateTime = lastChecked })
                    .ToList();

                    return list;

                }
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }


    }
}

