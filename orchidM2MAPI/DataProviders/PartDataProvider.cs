using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public class PartDataProvider : IPartDataProvider
    {
        private readonly IConfiguration _config;

        public PartDataProvider(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {

            return new SqlConnection(_config.GetConnectionString(location));

        }

        public async Task<Part> GetPart(string location, string partNo, string rev)
        {
            using (IDbConnection conn = Connection(location))
            {
                string sQuery = "SELECT RTRIM(fpartno) AS PartNo, RTRIM(frev) AS PartRev, RTRIM(fac) AS Facility, RTRIM(fgroup) AS PartFamily FROM dbo.inmast WHERE fpartNo = @partNo AND frev = @rev";
                conn.Open();
                var result = await conn.QueryAsync<Part>(sQuery, new { partNo = partNo, rev = rev });

                return result.FirstOrDefault();
                //QueryFirstOrDefaultAsync
            }
        }

        public async Task<List<Part>> GetPart(string location, string partNo)
        {
            using (IDbConnection conn = Connection(location))
            {
                string sQuery = "SELECT RTRIM(fpartno) AS PartNo, RTRIM(frev) AS PartRev, RTRIM(fac) AS Facility, RTRIM(fgroup) AS PartFamily FROM dbo.inmast WHERE fpartNo = @partNo";
                conn.Open();
                var result = await conn.QueryAsync<Part>(sQuery, new { partNo = partNo});

                return result.ToList();
                //QueryFirstOrDefaultAsync
            }
        }

    }
}

