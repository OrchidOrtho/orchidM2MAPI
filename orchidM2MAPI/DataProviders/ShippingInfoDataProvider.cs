﻿using System;
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
    public class ShippingInfoDataProvider : IShippingInfoDataProvider
    {
        private readonly IConfiguration _config;

        public ShippingInfoDataProvider(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {

            return new SqlConnection(_config.GetConnectionString(location));

        }

        public async Task<ShippingAllInfo> GetShippingInfo(string location, string shipperNo)
        {
            using (IDbConnection conn = Connection(location))
            {

                string sQuery = "SELECT dbo.shmast.fshipno, dbo.shmast.fshipdate, dbo.shitem.fpartno, dbo.shitem.frev, dbo.shitem.fshipqty, dbo.shitem.fmeasure, dbo.shlotc.fclot, dbo.shlotc.fnlotqty, LEFT(dbo.shitem.fsokey, 6) AS SaleOrderNumber, dbo.somast.fcustpono, dbo.inmast.fdescript, dbo.soitem.fcustpart, dbo.SOITEM_EXT.CPOLINE AS CustomerPOLineNumber, dbo.shmast.fcnumber AS CustomerNumber, dbo.soitem.fenumber AS SalesOrderLineNumber FROM dbo.shmast INNER JOIN dbo.shitem ON dbo.shmast.fshipno = dbo.shitem.fshipno INNER JOIN dbo.somast ON LEFT(dbo.shitem.fsokey, 6) = dbo.somast.fsono INNER JOIN dbo.shlotc ON dbo.shitem.fshipno = dbo.shlotc.fcshipno AND dbo.shitem.fitemno = dbo.shlotc.fcitemno INNER JOIN dbo.inmast ON dbo.shitem.fpartno = dbo.inmast.fpartno AND dbo.shitem.frev = dbo.inmast.frev INNER JOIN dbo.soitem ON dbo.somast.fsono = dbo.soitem.fsono AND dbo.shitem.fpartno = dbo.soitem.fpartno AND dbo.shitem.frev = dbo.soitem.fpartrev LEFT OUTER JOIN dbo.SOITEM_EXT ON dbo.soitem.identity_column = dbo.SOITEM_EXT.FKey_ID WHERE (dbo.shmast.fshipno = @shipperNo) ORDER BY dbo.shlotc.fclot";
                //conn.Open();
                var result = await conn.QueryAsync<ShippingInfo>(sQuery, new { shipperNo = shipperNo });

                string sQueryLotInfo = "WITH LotData AS (SELECT dbo.shlotc.fclot AS ShipperLot, dbo.shlotc.fclot, dbo.qalotc.fcuseindoc, dbo.qalotc.fcpartno, dbo.qalotc.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase FROM dbo.qalotc INNER JOIN dbo.shlotc ON dbo.qalotc.fclot = dbo.shlotc.fclot INNER JOIN dbo.inmast ON dbo.qalotc.fcpartno = dbo.inmast.fpartno AND dbo.qalotc.fcpartrev = dbo.inmast.frev WHERE (dbo.shlotc.fcshipno = @shipperNo) UNION ALL SELECT ld.ShipperLot, q.fclot, q.fcuseindoc, q.fcpartno, q.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase FROM dbo.qalotc AS q INNER JOIN LotData AS ld ON q.fcuseindoc = RTRIM(ld.fclot) + '-0000' INNER JOIN dbo.inmast ON q.fcpartno = dbo.inmast.fpartno AND q.fcpartrev = dbo.inmast.frev ) SELECT ShipperLot, fclot, fcpartno, fcpartrev, fdescript, fcpurchase FROM LotData GROUP BY ShipperLot, fclot, fcpartno, fcpartrev, fdescript, fcpurchase";

                var resultLotInfo = await conn.QueryAsync<ShippingLotInfo>(sQueryLotInfo, new { shipperNo = shipperNo });

                ShippingAllInfo returnVal = new ShippingAllInfo();
                returnVal.LotInfoList = resultLotInfo.ToList();
                returnVal.ShippingInfoList = result.ToList();

                return returnVal;
            }
        }
    }
}