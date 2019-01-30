using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using orchidM2MAPI.Models;
using Dapper;

namespace orchidM2MAPI.DataProviders
{
    public class ShippingDataProvider : IShippingDataProvider
    {
        private readonly IConfigurationRoot _config;

        public ShippingDataProvider(IConfigurationRoot config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:" + location;
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);

        }

        public async Task<Shipping> GetShipping(string location, string shipperNo)
        {
                 string sQuery = "";

                // Retrieve shipping info
                switch (location)
                {
                    case "010":
                    //Adds laser etch field from the job extended table
                        sQuery = "WITH LotData AS (SELECT        LEFT(dbo.qalotc.fcuseindoc, 6) AS ShipperNo, dbo.qalotc.fclot AS ShipperLot, dbo.qalotc.fclot, dbo.qalotc.fcpartno, dbo.qalotc.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase, dbo.qalotc.fac, dbo.qalotc.fnquantity, dbo.qalotc.fcmeasure, 1 AS BOMLevel, dbo.inmast.fgroup FROM            dbo.qalotc INNER JOIN                          dbo.inmast ON dbo.qalotc.fcpartno = dbo.inmast.fpartno AND dbo.qalotc.fcpartrev = dbo.inmast.frev AND dbo.qalotc.fac = dbo.inmast.fac WHERE        (dbo.qalotc.fctype = 'I') AND (LEFT(dbo.qalotc.fcuseindoc, 6) = @shipperNo) UNION ALL SELECT  ld.ShipperNo, ld.ShipperLot, qalotc_1.fclot, qalotc_1.fcpartno, qalotc_1.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase, dbo.inmast.fac, qalotc_1.fnquantity, qalotc_1.fcmeasure, ld.BOMLevel + 1 AS BOMLevel, dbo.inmast.fgroup FROM            dbo.qalotc INNER JOIN                          dbo.qalotc AS qalotc_1 ON dbo.qalotc.fcdoc = qalotc_1.fcuseindoc INNER JOIN                          dbo.inmast ON qalotc_1.fcpartno = dbo.inmast.fpartno AND qalotc_1.fcpartrev = dbo.inmast.frev AND qalotc_1.fac = dbo.inmast.fac INNER JOIN LotData as ld on qalotc.fclot = ld.fclot WHERE  (dbo.qalotc.fctype = 'J')  )  SELECT  dbo.shmast.identity_column AS ShippingId, dbo.shmast.fshipno AS ShipperNo, RTRIM(dbo.shmast.fshipvia) AS ShipVia, dbo.shmast.ftype AS ShipType, dbo.shmast.fcnumber AS CustomerNo, dbo.shmast.fshipdate AS ShipDate, dbo.shitem.identity_column AS ShippingItemId, dbo.shitem.fitemno AS ShippingItemNo, RTRIM(dbo.shitem.fpartno) AS PartNo, RTRIM(dbo.shitem.frev) AS PartRev, RTRIM(dbo.shitem.fac) AS Facility, dbo.shitem.fshipqty AS ShipQuantity, RTRIM(dbo.shitem.fmeasure) AS ShipUnitofMeasure,                          SORelease.fsono AS SalesOrderNo, SORelease.frelease AS SalesOrderReleaseNo, SORelease.finumber AS SalesOrderReleaseLineI, SORelease.fenumber AS SalesOrderReleaseLineE, RTRIM(dbo.somast.fcustpono) AS CustomerPONo,                           dbo.SOITEM_EXT.CPOLINE AS CustomerPOLineNo, RTRIM(dbo.soitem.fcustpart) AS CustomerPartNo, RTRIM(dbo.soitem.fcustptrev) AS CustomerPartRev, RTRIM(dbo.inmast.fdescript) AS PartDesc, dbo.shlotc.identity_column AS ShippingItemLotId, RTRIM(dbo.shlotc.fclot) AS LotNo, dbo.shlotc.fnlotqty AS LotQuantity, RTRIM(Job.LASRETCH) AS LaserEtch, LotData.BOMLevel,  RTRIM(LotData.fclot) AS BOMLotNo, RTRIM(LotData.fcpartno) AS BOMPartNo, RTRIM(LotData.fcpartrev) AS BOMPartRev, RTRIM(LotData.fdescript) AS BOMPartDesc, LotData.fcpurchase AS Purchased, LotData.fnquantity AS BOMQuantity, RTRIM(LotData.fcmeasure) AS BOMUnitofMeasure, RTRIM(LotData.fgroup) AS PartGroup FROM            dbo.shmast INNER JOIN                          dbo.shitem ON dbo.shmast.fshipno = dbo.shitem.fshipno INNER JOIN                          dbo.somast ON LEFT(dbo.shitem.fsokey, 6) = dbo.somast.fsono INNER JOIN                          dbo.shlotc ON dbo.shitem.fshipno = dbo.shlotc.fcshipno AND dbo.shitem.fitemno = dbo.shlotc.fcitemno INNER JOIN                          dbo.inmast ON dbo.shitem.fpartno = dbo.inmast.fpartno AND dbo.shitem.frev = dbo.inmast.frev INNER JOIN                          dbo.soitem ON dbo.somast.fsono = dbo.soitem.fsono INNER JOIN                              (SELECT        fsono, frelease, finumber, fenumber, fsono + finumber + frelease AS PKey                                FROM            dbo.sorels) AS SORelease ON dbo.shitem.fsokey = SORelease.PKey AND dbo.soitem.fsono = SORelease.fsono AND dbo.soitem.finumber = SORelease.finumber LEFT OUTER JOIN                          dbo.SOITEM_EXT ON dbo.soitem.identity_column = dbo.SOITEM_EXT.FKey_ID LEFT OUTER JOIN                              (SELECT        dbo.jomast.fjobno, dbo.JOMAST_EXT.LASRETCH, dbo.qalotc.fclot                                FROM            dbo.JOMAST_EXT INNER JOIN                                                          dbo.jomast ON dbo.JOMAST_EXT.FKey_ID = dbo.jomast.identity_column INNER JOIN                                                          dbo.qalotc ON dbo.jomast.fjobno = dbo.qalotc.fcdoc                                WHERE        (dbo.qalotc.fctype = 'J')) AS Job ON dbo.shlotc.fclot = Job.fclot INNER JOIN LotData 							     ON LotData.ShipperNo = dbo.shitem.fshipno WHERE        dbo.shmast.fshipno = @shipperNo";
                        break;
                    default:
                        sQuery = "WITH LotData AS (SELECT        LEFT(dbo.qalotc.fcuseindoc, 6) AS ShipperNo, dbo.qalotc.fclot AS ShipperLot, dbo.qalotc.fclot, dbo.qalotc.fcpartno, dbo.qalotc.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase, dbo.qalotc.fac, dbo.qalotc.fnquantity, dbo.qalotc.fcmeasure, 1 AS BOMLevel, dbo.inmast.fgroup FROM            dbo.qalotc INNER JOIN                          dbo.inmast ON dbo.qalotc.fcpartno = dbo.inmast.fpartno AND dbo.qalotc.fcpartrev = dbo.inmast.frev AND dbo.qalotc.fac = dbo.inmast.fac WHERE        (dbo.qalotc.fctype = 'I') AND (LEFT(dbo.qalotc.fcuseindoc, 6) = @shipperNo) UNION ALL SELECT  ld.ShipperNo, ld.ShipperLot, qalotc_1.fclot, qalotc_1.fcpartno, qalotc_1.fcpartrev, dbo.inmast.fdescript, dbo.inmast.fcpurchase, dbo.inmast.fac, qalotc_1.fnquantity, qalotc_1.fcmeasure, ld.BOMLevel + 1 AS BOMLevel, dbo.inmast.fgroup FROM            dbo.qalotc INNER JOIN                          dbo.qalotc AS qalotc_1 ON dbo.qalotc.fcdoc = qalotc_1.fcuseindoc INNER JOIN                          dbo.inmast ON qalotc_1.fcpartno = dbo.inmast.fpartno AND qalotc_1.fcpartrev = dbo.inmast.frev AND qalotc_1.fac = dbo.inmast.fac INNER JOIN LotData as ld on qalotc.fclot = ld.fclot WHERE  (dbo.qalotc.fctype = 'J')  )  SELECT  dbo.shmast.identity_column AS ShippingId, dbo.shmast.fshipno AS ShipperNo, RTRIM(dbo.shmast.fshipvia) AS ShipVia, dbo.shmast.ftype AS ShipType, dbo.shmast.fcnumber AS CustomerNo, dbo.shmast.fshipdate AS ShipDate, dbo.shitem.identity_column AS ShippingItemId, dbo.shitem.fitemno AS ShippingItemNo, RTRIM(dbo.shitem.fpartno) AS PartNo, RTRIM(dbo.shitem.frev) AS PartRev, RTRIM(dbo.shitem.fac) AS Facility, dbo.shitem.fshipqty AS ShipQuantity, RTRIM(dbo.shitem.fmeasure) AS ShipUnitofMeasure,                          SORelease.fsono AS SalesOrderNo, SORelease.frelease AS SalesOrderReleaseNo, SORelease.finumber AS SalesOrderReleaseLineI, SORelease.fenumber AS SalesOrderReleaseLineE, RTRIM(dbo.somast.fcustpono) AS CustomerPONo,                           dbo.SOITEM_EXT.CPOLINE AS CustomerPOLineNo, RTRIM(dbo.soitem.fcustpart) AS CustomerPartNo, RTRIM(dbo.soitem.fcustptrev) AS CustomerPartRev, RTRIM(dbo.inmast.fdescript) AS PartDesc, dbo.shlotc.identity_column AS ShippingItemLotId, RTRIM(dbo.shlotc.fclot) AS LotNo, dbo.shlotc.fnlotqty AS LotQuantity, LotData.BOMLevel,  RTRIM(LotData.fclot) AS BOMLotNo, RTRIM(LotData.fcpartno) AS BOMPartNo, RTRIM(LotData.fcpartrev) AS BOMPartRev, RTRIM(LotData.fdescript) AS BOMPartDesc, LotData.fcpurchase AS Purchased, LotData.fnquantity AS BOMQuantity, RTRIM(LotData.fcmeasure) AS BOMUnitofMeasure, RTRIM(LotData.fgroup) AS PartGroup FROM            dbo.shmast INNER JOIN                          dbo.shitem ON dbo.shmast.fshipno = dbo.shitem.fshipno INNER JOIN                          dbo.somast ON LEFT(dbo.shitem.fsokey, 6) = dbo.somast.fsono INNER JOIN                          dbo.shlotc ON dbo.shitem.fshipno = dbo.shlotc.fcshipno AND dbo.shitem.fitemno = dbo.shlotc.fcitemno INNER JOIN                          dbo.inmast ON dbo.shitem.fpartno = dbo.inmast.fpartno AND dbo.shitem.frev = dbo.inmast.frev INNER JOIN                          dbo.soitem ON dbo.somast.fsono = dbo.soitem.fsono INNER JOIN                              (SELECT        fsono, frelease, finumber, fenumber, fsono + finumber + frelease AS PKey                                FROM            dbo.sorels) AS SORelease ON dbo.shitem.fsokey = SORelease.PKey AND dbo.soitem.fsono = SORelease.fsono AND dbo.soitem.finumber = SORelease.finumber LEFT OUTER JOIN                          dbo.SOITEM_EXT ON dbo.soitem.identity_column = dbo.SOITEM_EXT.FKey_ID INNER JOIN LotData 							     ON LotData.ShipperNo = dbo.shitem.fshipno WHERE        dbo.shmast.fshipno = @shipperNo";
                        break;
                }


                using (var connection = Connection(location))
                {
                    var shippingDictionary = new Dictionary<int, Shipping>();
                    var shippingItemDictionary = new Dictionary<int, ShippingItem>();
                    var shippingLotDictionary = new Dictionary<int, ShippingLot>();


                var list = connection.Query<Shipping, ShippingItem, ShippingLot, ShippingLotBOM, Shipping>(
                        sQuery,
                        (shipping, shippingitem, shippinglot, shippinglotbom) =>
                        {
                            Shipping shippingEntry;
                            ShippingItem shippingItemEntry;
                            ShippingLot shippingLotEntry;

                            if (!shippingDictionary.TryGetValue(shipping.ShippingId, out shippingEntry))
                            {
                                shippingEntry = shipping;
                                shippingEntry.ShippingItems = new List<ShippingItem>();
                                shippingDictionary.Add(shippingEntry.ShippingId, shippingEntry);
                                shippingEntry.ShippingItems.Add(shippingitem);
                            }
                            else
                            {
                                if (!shippingEntry.ShippingItems.Exists( x => x.ShippingItemId == shippingitem.ShippingItemId))
                                {
                                    shippingEntry.ShippingItems.Add(shippingitem);
                                }
                            }


                            if (!shippingItemDictionary.TryGetValue(shippingitem.ShippingItemId, out shippingItemEntry))
                            {
                                shippingItemEntry = shippingitem;
                                shippingItemEntry.ShippingItemLots = new List<ShippingLot>();
                                shippingItemDictionary.Add(shippingitem.ShippingItemId, shippingItemEntry);
                                shippingItemEntry.ShippingItemLots.Add(shippinglot);
                            }
                            else
                            {
                                if (!shippingItemEntry.ShippingItemLots.Exists(x => x.ShippingItemLotId == shippinglot.ShippingItemLotId))
                                {
                                    shippingItemEntry.ShippingItemLots.Add(shippinglot);
                                }
                            }
                            

                            if (!shippingLotDictionary.TryGetValue(shippinglot.ShippingItemLotId, out shippingLotEntry))
                            {
                                shippingLotEntry = shippinglot;
                                shippingLotEntry.ShippingItemLotBOM = new List<ShippingLotBOM>();
                                shippingLotDictionary.Add(shippinglot.ShippingItemLotId, shippingLotEntry);
                                shippingLotEntry.ShippingItemLotBOM.Add(shippinglotbom);

                            }
                            else
                            {
                                if (!shippingLotEntry.ShippingItemLotBOM.Exists(x => x.BOMLotNo == shippinglotbom.BOMLotNo))
                                {
                                    shippingLotEntry.ShippingItemLotBOM.Add(shippinglotbom);
                                }
                            }

                            return shippingEntry;
                        },
                        param: new { shipperNo = shipperNo },
                        splitOn: "ShippingItemId, ShippingItemLotId, BOMLevel")
                    .Distinct()
                    .ToList();

                    return shippingDictionary.FirstOrDefault().Value;

                }
        }
    }
}
