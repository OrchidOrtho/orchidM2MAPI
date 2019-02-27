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
    public class SalesOrderDataProvider : ISalesOrderDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public SalesOrderDataProvider(IConfigurationRoot config, ILoggerFactory logger)
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

        public async Task<SalesOrder> GetSalesOrder(string location, string CustomerPONo, string CustomerNo)
        {
            try
            {
                string sQuery = "";

                // Retrieve sales order info
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "SELECT        dbo.co.co_num AS SalesOrderNo, dbo.co.cust_num AS CustomerNo, '' AS CustomerName, dbo.co.cust_po AS CustomerPONo, NULL AS AcknowledgedDate, dbo.co.order_date AS OrderDate, '' AS Status, - 99999 AS SalesOrderId,                          dbo.coitem.co_line AS InternalItemNo, dbo.coitem.item AS PartNo, dbo.item.revision AS PartRev, dbo.coitem.cust_item AS CustomerPartNo, '' AS CustomerPartRev, dbo.coitem.due_date AS DueDateToShip,                          '' AS SalesOrderItemNo, dbo.coitem.qty_ordered AS Quantity, dbo.coitem.u_m AS UnitOfMeasure, dbo.item.description AS PartDesc, dbo.item.family_code AS PartGroup FROM            dbo.co INNER JOIN                          dbo.coitem ON dbo.co.co_num = dbo.coitem.co_num INNER JOIN                          dbo.item ON dbo.coitem.item = dbo.item.item WHERE        (dbo.co.cust_num = @customerNo) AND (dbo.co.cust_po = @customerPONo)";
                        break;
                    default:
                        sQuery = "SELECT        TOP (100) PERCENT dbo.somast.fsono AS SalesOrderNo, dbo.somast.fcustno AS CustomerNo, RTRIM(dbo.somast.fcompany) AS CustomerName, RTRIM(dbo.somast.fcustpono) AS CustomerPONo, dbo.somast.fackdate AS AcknowledgedDate,                          dbo.somast.forderdate AS OrderDate, RTRIM(dbo.somast.fstatus) AS Status, dbo.somast.identity_column AS SalesOrderId, dbo.soitem.finumber AS InternalItemNo, RTRIM(dbo.soitem.fpartno) AS PartNo, RTRIM(dbo.soitem.fpartrev) AS PartRev,                          RTRIM(dbo.soitem.fcustpart) AS CustomerPartNo, RTRIM(dbo.soitem.fcustptrev) AS CustomerPartRev, dbo.soitem.fduedate AS DueDateToShip, dbo.soitem.fenumber AS SalesOrderItemNo, RTRIM(dbo.soitem.fmeasure) AS UnitOfMeasure,                          dbo.soitem.fquantity AS Quantity, dbo.soitem.identity_column AS SalesOrderItemId, RTRIM(dbo.soitem.fgroup) AS PartGroup, dbo.soitem.fdesc AS PartDesc FROM            dbo.somast INNER JOIN                          dbo.soitem ON dbo.somast.fsono = dbo.soitem.fsono WHERE        (dbo.somast.fcustpono = @customerPONo) AND (dbo.somast.fcustno = @customerNo)";
                        break;
                }


                using (var connection = Connection(location))
                {
                    var soDictionary = new Dictionary<int, SalesOrder>();


                    var list = connection.Query<SalesOrder, SalesOrderItem, SalesOrder>(
                        sQuery,
                        (so, soitem) =>
                        {
                            SalesOrder soEntry;

                            if (!soDictionary.TryGetValue(so.SalesOrderId, out soEntry))
                            {
                                soEntry = so;
                                soEntry.Items = new List<SalesOrderItem>();
                                soDictionary.Add(soEntry.SalesOrderId, soEntry);
                            }
                            soEntry.Items.Add(soitem);


                            return soEntry;
                        },
                        param: new { customerPONo = CustomerPONo, customerNo = CustomerNo },
                        splitOn: "InternalItemNo")
                    .Distinct()
                    .ToList();

                    return soDictionary.FirstOrDefault().Value;

                }
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }


        public async Task<SalesOrderItem> UpdateSalesOrderDetail(string location, SalesOrderItem item)
        {
            try
            {
                string sQuery = "";
                string sQuery1 = "";

                // Retrieve sales order info
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "";
                        break;
                    default:
                        sQuery = "UPDATE       TOP (1) sorels SET                fduedate = @newDueDate FROM            soitem INNER JOIN                          sorels ON soitem.finumber = sorels.finumber AND soitem.fsono = sorels.fsono WHERE        (soitem.identity_column = @uniqueId)";
                        sQuery1 = "UPDATE TOP (1) soitem SET fduedate = @newDueDate  WHERE   (identity_column = @uniqueId)";
                        break;
                }


                using (var connection = Connection(location))
                {
                    connection.Execute(sQuery, new {uniqueId = item.SalesOrderItemId, newDueDate = item.DueDateToShip });
                    connection.Execute(sQuery1, new { uniqueId = item.SalesOrderItemId, newDueDate = item.DueDateToShip });

                    return item;

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
