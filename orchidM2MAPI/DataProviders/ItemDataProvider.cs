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
    public class ItemDataProvider : IItemDataProvider
    {
        private readonly IConfiguration _config;

        public ItemDataProvider(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {

            return new SqlConnection(_config.GetConnectionString(location));

        }

        public async Task<Item> GetItem(string location, string partNo, string rev)
        {
            using (IDbConnection conn = Connection(location))
            {
                string sQuery = "SELECT fpartno, frev FROM dbo.inmast WHERE fpartNo = @partNo AND frev = @rev";
                conn.Open();
                var result = await conn.QueryAsync<Item>(sQuery, new { partNo = partNo, rev = rev });

                return result.FirstOrDefault();
                //QueryFirstOrDefaultAsync
            }
        }

    }
}

