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
    public class ShippingLotInfoDataProvider : IShippingLotInfoDataProvider
    {
        private readonly IConfiguration _config;

        public ShippingLotInfoDataProvider(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {

            return new SqlConnection(_config.GetConnectionString(location));

        }

        public async Task<List<ShippingLotInfo>> GetShippingLotInfo(string location, string shipperNo)
        {
            using (IDbConnection conn = Connection(location))
            {
                //string sQuery = "WITH LotData AS (SELECT TOP(100) PERCENT dbo.qalotc.fctype, dbo.qalotc.fcdoc, dbo.qalotc.fcpartno, dbo.qalotc.fcpartrev, dbo.qalotc.fclot, dbo.qalotc.fcuseintype, dbo.qalotc.fcuseindoc, dbo.qalotc.fcuseinpart, dbo.qalotc.fcuseinrev, dbo.qalotc.fcuseinlot, dbo.qalotc.fnquantity, dbo.qalotc.fcnumber, dbo.qalotc.fccompany, dbo.qalotc.fddate, dbo.qalotc.fcmeasure, dbo.qalotc.identity_column, dbo.qalotc.timestamp_column, dbo.qalotc.fac, dbo.qalotc.useinfac, dbo.qalotc.fcudrev FROM   dbo.shmast INNER JOIN dbo.shitem ON dbo.shmast.fshipno = dbo.shitem.fshipno INNER JOIN dbo.shsrce ON dbo.shitem.fitemno = dbo.shsrce.fcitemno AND dbo.shitem.fshipno = dbo.shsrce.fcshipno INNER JOIN dbo.shlotc ON dbo.shsrce.fcshipno = dbo.shlotc.fcshipno AND dbo.shsrce.fcitemno = dbo.shlotc.fcitemno INNER JOIN dbo.qalotc ON dbo.shlotc.fclot = dbo.qalotc.fclot WHERE(dbo.shmast.fshipno = @shipperNo) UNION ALL SELECT q.* FROM dbo.qalotc AS q INNER JOIN LotData AS ld ON q.fcuseindoc = RTRIM(ld.fclot) + '-0000' ) SELECT RTRIM(fclot) AS fclot, RTRIM(fcpartno) AS fcpartno, fcpartrev FROM LotData  GROUP BY RTRIM(fclot), RTRIM(fcpartno), fcpartrev";
                string sQuery = "WITH LotData AS (SELECT dbo.shlotc.fclot AS ShipperLot, dbo.shlotc.fclot, dbo.qalotc.fcuseindoc, dbo.qalotc.fcpartno, dbo.qalotc.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase FROM dbo.qalotc INNER JOIN dbo.shlotc ON dbo.qalotc.fclot = dbo.shlotc.fclot INNER JOIN dbo.inmast ON dbo.qalotc.fcpartno = dbo.inmast.fpartno AND dbo.qalotc.fcpartrev = dbo.inmast.frev WHERE (dbo.shlotc.fcshipno = @shipperNo) UNION ALL SELECT ld.ShipperLot, q.fclot, q.fcuseindoc, q.fcpartno, q.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase FROM dbo.qalotc AS q INNER JOIN LotData AS ld ON q.fcuseindoc = RTRIM(ld.fclot) + '-0000' INNER JOIN dbo.inmast ON q.fcpartno = dbo.inmast.fpartno AND q.fcpartrev = dbo.inmast.frev ) SELECT ShipperLot, fclot, fcpartno, fcpartrev, fdescript, fcpurchase FROM LotData GROUP BY ShipperLot, fclot, fcpartno, fcpartrev, fdescript, fcpurchase";
                conn.Open();
                var result = await conn.QueryAsync<ShippingLotInfo>(sQuery, new { shipperNo = shipperNo });

                return result.ToList();
            }
        }
    }
}
