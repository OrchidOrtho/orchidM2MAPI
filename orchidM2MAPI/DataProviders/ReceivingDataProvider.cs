﻿using System;
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
    public class ReceivingDataProvider : IReceivingDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public ReceivingDataProvider(IConfigurationRoot config, ILoggerFactory logger)
        {
            _config = config;
            _logger = logger;

        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:" + location;
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);

        }

        public async Task<Receiving> GetReceiving(string location, string ReceivingNo)
        {
            //string sQuery = "SELECT dbo.rcmast.freceiver AS ReceiverNo, dbo.rcmast.fvendno AS VendorNo, dbo.rcmast.identity_column AS ReceivingId, dbo.rcmast.fpono AS PONumber, dbo.rcmast.fdaterecv AS DateReceived, dbo.rcmast.fcstatus AS Status, dbo.rcmast.ftype AS Type, dbo.rcitem.identity_column AS ReceivingItemId, dbo.rcitem.ftype AS ReceivingItemType, dbo.rcitem.fitemno AS ReceivingItemLineNo, dbo.rcitem.fpartno AS PartNo, dbo.rcitem.fpartrev AS PartRev, dbo.rcitem.fac AS Facility, dbo.rcitem.fqtyrecv AS TotalQuantityReceived, dbo.rcitem.flocation AS InventoryLocation, dbo.rcitem.fmeasure AS QuantityUnitofMeasure, dbo.rcitem.fcomments AS Comments, dbo.rcitem.fpoitemno AS POItemNo, dbo.rcitem.finspect AS InspectFlag FROM dbo.rcitem INNER JOIN dbo.rcmast ON dbo.rcitem.freceiver = dbo.rcmast.freceiver WHERE (dbo.rcmast.freceiver = @receivingNo)";
            string sQuery = "SELECT        dbo.rcmast.freceiver AS ReceiverNo, dbo.rcmast.fvendno AS VendorNo, dbo.rcmast.identity_column AS ReceivingId, dbo.rcmast.fpono AS PONumber, dbo.rcmast.fdaterecv AS DateReceived, dbo.rcmast.fcstatus AS Status,                          dbo.rcmast.ftype AS Type, dbo.rcitem.identity_column AS ReceivingItemId, dbo.rcitem.ftype AS ReceivingItemType, dbo.rcitem.fitemno AS ReceivingItemLineNo, RTRIM(dbo.rcitem.fpartno) AS PartNo, RTRIM(dbo.rcitem.fpartrev)                          AS PartRev, RTRIM(dbo.rcitem.fac) AS Facility, dbo.rcitem.fqtyrecv AS TotalQuantityReceived, RTRIM(dbo.rcitem.flocation) AS InventoryLocation, RTRIM(dbo.rcitem.fmeasure) AS QuantityUnitofMeasure,                          dbo.rcitem.fcomments AS Comments, dbo.rcitem.fpoitemno AS POItemNumber, dbo.rcitem.fjokey AS JobNo, dbo.rcitem.finspect AS InspectFlag, poItem.fordqty AS OrderedQuantity, poItem.fmeasure AS OrderedUnitofMeasure,                          RTRIM(LotData.fclot) AS LotNumber, LotData.fnlotqty AS LotQuantity, LotData.fdexpdate AS ExpirationDate FROM            dbo.rcitem INNER JOIN                          dbo.rcmast ON dbo.rcitem.freceiver = dbo.rcmast.freceiver LEFT OUTER JOIN                              (SELECT        dbo.pomast.fpono, poitem_1.fitemno, poitem_1.frelsno, poitem_1.fordqty, poitem_1.fmeasure                                FROM            dbo.pomast INNER JOIN                                                          dbo.poitem AS poitem_1 ON dbo.pomast.fpono = poitem_1.fpono) AS poItem ON dbo.rcmast.fpono = poItem.fpono AND dbo.rcitem.fpoitemno = poItem.fitemno AND                          dbo.rcitem.frelsno = poItem.frelsno LEFT OUTER JOIN                              (SELECT        SUBSTRING(fcrcitmkey, 1, 6) AS receivingID, SUBSTRING(fcrcitmkey, 7, 3) AS receivingItemID, fclot, fdexpdate, fnlotqty                                FROM            dbo.rclotc) AS LotData ON dbo.rcitem.freceiver = LotData.receivingID AND dbo.rcitem.fitemno = LotData.receivingItemID WHERE        (dbo.rcmast.freceiver = @receivingNo)";
            using (var connection = Connection(location))
            {
                var receivingDictionary = new Dictionary<int, Receiving>();
                var receivingItemDictionary = new Dictionary<int, ReceivingItem>();


                var list = connection.Query<Receiving, ReceivingItem, ReceivingItemLot, Receiving>(
                    sQuery,  
                    (receiving, receivingitems, receivinglots) =>
                    {
                        Receiving receivingEntry;
                        ReceivingItem receivingItemEntry;

                        if (!receivingDictionary.TryGetValue(receiving.ReceivingId, out receivingEntry))
                        {
                            receivingEntry = receiving;
                            receivingEntry.ReceivingItems = new List<ReceivingItem>();
                            receivingDictionary.Add(receivingEntry.ReceivingId, receivingEntry);
                        }
                        receivingEntry.ReceivingItems.Add(receivingitems);

                        if (!receivingItemDictionary.TryGetValue(receivingitems.ReceivingItemId, out receivingItemEntry))
                        {
                            receivingItemEntry = receivingitems;
                            receivingItemEntry.Lots = new List<ReceivingItemLot>();
                            receivingItemDictionary.Add(receivingitems.ReceivingItemId, receivingitems);
                        }
                        receivingItemEntry.Lots.Add(receivinglots);

                        return receivingEntry;
                    },
                    param: new { ReceivingNo = ReceivingNo },
                    splitOn: "ReceivingItemId, LotNumber")
                .Distinct()
                .ToList();

                return receivingDictionary.FirstOrDefault().Value;
            }
        }


        public async Task<List<Receiving>> GetReceivedItems(string location, string PONo)
        {
            string sQuery = "SELECT        dbo.rcmast.freceiver AS ReceiverNo, dbo.rcmast.fvendno AS VendorNo, dbo.rcmast.identity_column AS ReceivingId, dbo.rcmast.fpono AS PONumber, dbo.rcmast.fdaterecv AS DateReceived, dbo.rcmast.fcstatus AS Status,                          dbo.rcmast.ftype AS Type, dbo.rcitem.identity_column AS ReceivingItemId, dbo.rcitem.ftype AS ReceivingItemType, dbo.rcitem.fitemno AS ReceivingItemLineNo, RTRIM(dbo.rcitem.fpartno) AS PartNo, RTRIM(dbo.rcitem.fpartrev) AS PartRev,                          RTRIM(dbo.rcitem.fac) AS Facility, dbo.rcitem.fqtyrecv AS TotalQuantityReceived, RTRIM(dbo.rcitem.flocation) AS InventoryLocation, RTRIM(dbo.rcitem.fmeasure) AS QuantityUnitofMeasure, dbo.rcitem.fcomments AS Comments,                          dbo.rcitem.fpoitemno AS POItemNumber, dbo.rcitem.fjokey AS JobNo, dbo.rcitem.finspect AS InspectFlag, RTRIM(LotData.fclot) AS LotNumber, LotData.fnlotqty AS LotQuantity, LotData.fdexpdate AS ExpirationDate FROM            dbo.rcitem INNER JOIN                          dbo.rcmast ON dbo.rcitem.freceiver = dbo.rcmast.freceiver LEFT OUTER JOIN                              (SELECT        SUBSTRING(fcrcitmkey, 1, 6) AS receivingID, SUBSTRING(fcrcitmkey, 7, 3) AS receivingItemID, fclot, fdexpdate, fnlotqty                                FROM            dbo.rclotc) AS LotData ON dbo.rcitem.freceiver = LotData.receivingID AND dbo.rcitem.fitemno = LotData.receivingItemID WHERE        (dbo.rcmast.fpono  = @poNo)";
            using (var connection = Connection(location))
            {
                var receivingDictionary = new Dictionary<int, Receiving>();
                var receivingItemDictionary = new Dictionary<int, ReceivingItem>();


                var list = connection.Query<Receiving, ReceivingItem, ReceivingItemLot, Receiving>(
                    sQuery,
                    (receiving, receivingitems, receivinglots) =>
                    {
                        Receiving receivingEntry;
                        ReceivingItem receivingItemEntry;

                        if (!receivingDictionary.TryGetValue(receiving.ReceivingId, out receivingEntry))
                        {
                            receivingEntry = receiving;
                            receivingEntry.ReceivingItems = new List<ReceivingItem>();
                            receivingDictionary.Add(receivingEntry.ReceivingId, receivingEntry);
                        }
                        receivingEntry.ReceivingItems.Add(receivingitems);

                        if (!receivingItemDictionary.TryGetValue(receivingitems.ReceivingItemId, out receivingItemEntry))
                        {
                            receivingItemEntry = receivingitems;
                            receivingItemEntry.Lots = new List<ReceivingItemLot>();
                            receivingItemDictionary.Add(receivingitems.ReceivingItemId, receivingitems);
                        }
                        receivingItemEntry.Lots.Add(receivinglots);

                        return receivingEntry;
                    },
                    param: new { poNo = PONo },
                    splitOn: "ReceivingItemId, LotNumber")
                .Distinct()
                .ToList();

                return receivingDictionary.Values.ToList();
            }
        }

        public async Task<List<Receiving>> GetReceivingSinceLastChecked(string location, DateTime lastChecked)
        {
            try
            {
                string sQuery = "";

                sQuery = "SELECT dbo.rcmast.identity_column AS ReceivingId, freceiver AS ReceiverNo, fdaterecv AS DateReceived FROM  dbo.rcmast WHERE (dbo.rcmast.fdaterecv >= @lastDateTime)";


                using (var connection = Connection(location))
                {

                    var list = connection.Query<Receiving>(
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

