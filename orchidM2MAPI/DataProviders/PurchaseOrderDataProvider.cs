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
    public class PurchaseOrderDataProvider : IPurchaseOrderDataProvider
    {
        private readonly IConfigurationRoot _config;

        public PurchaseOrderDataProvider(IConfigurationRoot config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:" + location;
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);

        }

        public async Task<PurchaseOrder> GetPurchaseOrder(string location, string poNo, string poItemReleaseNo)
        {
            using (IDbConnection conn = Connection(location))
            {
                string sQuery = "SELECT        TOP (100) PERCENT dbo.pomast.fcompany, dbo.pomast.forddate, dbo.pomast.fpono, dbo.pomast.fstatus, dbo.pomast.fvendno, dbo.poitem.fpartno, dbo.poitem.frev, dbo.poitem.fitemno, dbo.poitem.fnqtydm, dbo.poitem.fordqty, dbo.poitem.fqtyutol, dbo.poitem.fqtyltol, dbo.poitem.fvmeasure, dbo.poitem.finspect FROM dbo.pomast INNER JOIN dbo.poitem ON dbo.pomast.fpono = dbo.poitem.fpono WHERE  (dbo.pomast.fpono = @poNo) AND (LTRIM(dbo.poitem.frelsno) = @poItemReleaseNo) ORDER BY dbo.pomast.forddate DESC";
                conn.Open();
                var result = await conn.QueryAsync<PurchaseOrder>(sQuery, new { poNo = poNo, poItemReleaseNo = poItemReleaseNo });

                return result.FirstOrDefault();
            }
        }
    }
}
