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
using orchidM2MAPI.Utilities;

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
                        sQuery = "SELECT        dbo.co.co_num AS SalesOrderNo, dbo.co.cust_num AS CustomerNo, '' AS CustomerName, dbo.co.cust_po AS CustomerPONo, NULL AS AcknowledgedDate, dbo.co.order_date AS OrderDate, '' AS Status, - 99999 AS SalesOrderId,                          dbo.coitem.co_line AS InternalItemNo, dbo.coitem.item AS PartNo, dbo.item.revision AS PartRev, dbo.coitem.cust_item AS CustomerPartNo, '' AS CustomerPartRev, dbo.coitem.due_date AS DueDateToShip,                          '' AS SalesOrderItemNo, dbo.coitem.qty_ordered AS Quantity, dbo.coitem.u_m AS UnitOfMeasure, dbo.item.description AS PartDesc, dbo.item.family_code AS PartGroup FROM            dbo.co INNER JOIN                          dbo.coitem ON dbo.co.co_num = dbo.coitem.co_num INNER JOIN                          dbo.item ON dbo.coitem.item = dbo.item.item WHERE        (LTRIM(dbo.co.cust_num) = @customerNo) AND (dbo.co.cust_po = @customerPONo)";
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
                                soEntry.SalesOrderLineItems = new List<SalesOrderItem>();
                                soDictionary.Add(soEntry.SalesOrderId, soEntry);
                            }
                            soEntry.SalesOrderLineItems.Add(soitem);


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
                    connection.Execute(sQuery, new { uniqueId = item.SalesOrderItemId, newDueDate = item.DueDateToShip });
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


        public async Task<SalesOrder> UpsertSalesOrder(string location, SalesOrder so)
        {
            IDbTransaction transaction;
            bool NewInsert = false;

            try
            {
                isValid(so);

                using (var connection = Connection(location))
                {
                    connection.Open();
                    using (transaction = connection.BeginTransaction())
                    {
                        if (so.IsNew)
                        {
                            //Insert
                            this.insertSO(location, so, connection, transaction);
                            NewInsert = true;
                        }
                        else
                        {
                            //Update
                            this.updateSO(location, so, connection, transaction);
                        }

                        //Update the costs
                        this.setCosts(location, so, connection, transaction);

                        try
                        {
                            transaction.Commit();
                            connection.Close();

                            //If Insert, need to insert into the custom tables
                            if (NewInsert)
                            {
                                using (var connInsertEXT = Connection(location))
                                {
                                    connInsertEXT.Open();
                                    using (transaction = connInsertEXT.BeginTransaction())
                                    {
                                        SalesOrder justAddedSO = this.SelectSO(location, so);

                                        // Need to update any fields in the EXT tables based on the sales order sent from Mendix
                                        // Currently there's only the PO Line number field in the items
                                        foreach (SalesOrderItem i in so.SalesOrderLineItems)
                                        {
                                            justAddedSO.SalesOrderLineItems.FirstOrDefault(X => X.InternalItemNo == i.InternalItemNo).POLineNo = i.POLineNo;

                                        }

                                        this.insertSOExt(location, justAddedSO, connInsertEXT, transaction);

                                        try
                                        {
                                            transaction.Commit();
                                            connInsertEXT.Close();
                                     
                                        }
                                        catch (Exception exEXTCommit)
                                        {
                                            transaction.Rollback();
                                            throw exEXTCommit;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exCommit)
                        {
                            transaction.Rollback();
                            throw exCommit;
                        }
                    }


                }

                return this.SelectSO(location, so);
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }
        }

        public async Task<SalesOrder> SalesOrderStatus(string location, SalesOrder so )
        {
            IDbTransaction transaction;
            bool NewInsert = false;

            try
            {
                isValid(so);

                using (var connection = Connection(location))
                {
                    connection.Open();
                    using (transaction = connection.BeginTransaction())
                    {
                        if (so.IsNew)
                        {
                            //Insert
                            this.insertSO(location, so, connection, transaction);
                            NewInsert = true;
                        }
                        else
                        {
                            //Update
                            this.updateSO(location, so, connection, transaction);
                        }

                        //Update the costs
                        this.setCosts(location, so, connection, transaction);

                        try
                        {
                            transaction.Commit();
                            connection.Close();

                            //If Insert, need to insert into the custom tables
                            if (NewInsert)
                            {
                                using (var connInsertEXT = Connection(location))
                                {
                                    connInsertEXT.Open();
                                    using (transaction = connInsertEXT.BeginTransaction())
                                    {
                                        SalesOrder justAddedSO = this.SelectSO(location, so);

                                        // Need to update any fields in the EXT tables based on the sales order sent from Mendix
                                        // Currently there's only the PO Line number field in the items
                                        foreach (SalesOrderItem i in so.SalesOrderLineItems)
                                        {
                                            justAddedSO.SalesOrderLineItems.FirstOrDefault(X => X.InternalItemNo == i.InternalItemNo).POLineNo = i.POLineNo;

                                        }

                                        this.insertSOExt(location, justAddedSO, connInsertEXT, transaction);

                                        try
                                        {
                                            transaction.Commit();
                                            connInsertEXT.Close();

                                        }
                                        catch (Exception exEXTCommit)
                                        {
                                            transaction.Rollback();
                                            throw exEXTCommit;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exCommit)
                        {
                            transaction.Rollback();
                            throw exCommit;
                        }
                    }


                }

                return this.SelectSO(location, so);
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }
        }


        private void insertSO(string location, SalesOrder so, IDbConnection conn, IDbTransaction trans)
        {
            try
            {
                string sQuerySOInsert = "";
                string sQuerySOItemInsert = "";
                string sQuerySORelInsert = "";
                string sNextSONoQuery = "";
                string sNextSONoUpdateQuery = "";

                // Generate queries based on location
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuerySOInsert = "";
                        break;
                    case "080":
                        //Need different query for 7.5

                        sQuerySOInsert = "INSERT INTO somast (fsono, fcustno, fcompany, fcity, fcustpono, fackdate, fcanc_dt, fcontact, fcountry, fcusrchr1, fcusrchr2, fcusrchr3, fduedate, fdusrdate1, ffax, ffob, fnusrqty1, fnusrcur1, forderdate, fpaytype, fphone, fshipvia, fshptoaddr, fsocoord, fsoldaddr, fsoldby, fstate, fstatus, fterm, fterr, fzip, fncancchrge, fackmemo, fmstreet, fmusrmemo1, fpriority, CreatedDate, ModifiedDate, fbilladdr) ";
                        sQuerySOInsert = sQuerySOInsert + "VALUES (@fsono, @fcustno, @fcompany, @fcity, @fcustpono, @fackdate, @fcanc_dt, @fcontact, @fcountry, @fcusrchr1, @fcusrchr2, @fcusrchr3, @fduedate, @fdusrdate1, @ffax, @ffob, @fnusrqty1, @fnusrcur1, @forderdate, @fpaytype, @fphone, @fshipvia, @fshptoaddr, @fsocoord, @fsoldaddr, @fsoldby, @fstate, @fstatus, @fterm, @fterr, @fzip, @fncancchrge, @fackmemo, @fmstreet, @fmusrmemo1, @fpriority, @CreatedDate, @ModifiedDate, @fbilladdr)";

                        sNextSONoQuery = "SELECT  fcnumber AS SalesOrderNo FROM  dbo.sysequ WHERE  (fcclass = 'SOMAST.FSONO')";

                        sNextSONoUpdateQuery = "UPDATE dbo.sysequ SET fcnumber = @newSONumber WHERE  (fcclass = 'SOMAST.FSONO')";

                        break;
                    default:

                        sQuerySOInsert = "INSERT INTO somast (fsono, fcustno, fcompany, fcity, fcustpono, fackdate, fcanc_dt, fcontact, fcountry, fcusrchr1, fcusrchr2, fcusrchr3, fduedate, fdusrdate1, ffax, ffob, fnusrqty1, fnusrcur1, forderdate, fpaytype, fphone, fshipvia, fshptoaddr, fsocoord, fsoldaddr, fsoldby, fstate, fstatus, fterm, fterr, fzip, fackmemo, fmstreet, fmusrmemo1, fpriority, festimator, fnextenum, fnextinum, fsorev) ";
                        sQuerySOInsert = sQuerySOInsert + "VALUES (@fsono, @fcustno, @fcompany, @fcity, @fcustpono, @fackdate, @fcanc_dt, @fcontact, @fcountry, @fcusrchr1, @fcusrchr2, @fcusrchr3, @fduedate, @fdusrdate1, @ffax, @ffob, @fnusrqty1, @fnusrcur1, @forderdate, @fpaytype, @fphone, @fshipvia, @fshptoaddr, @fsocoord, @fsoldaddr, @fsoldby, @fstate, @fstatus, @fterm, @fterr, @fzip, @fackmemo, @fmstreet, @fmusrmemo1, @fpriority, @festimator, @fnextenum, @fnextinum, @fsorev)";

                        sQuerySOItemInsert = "INSERT INTO soitem (finumber, fpartno, fpartrev, fsono, fllotreqd, fautocreat, fcas_bom, fcas_rtg, fcustpart, fcustptrev, fduedate, fenumber, fgroup, fmeasure, fmultiple, fnextinum, fnextrel, fnunder, fnover, fordertype, fprintmemo, fprodcl, fquantity, fsource, fdesc, fdescmemo, fac, fclotext, fstandpart, sfac, FcAltUM, FnAltQty, fcudrev) ";
                        sQuerySOItemInsert = sQuerySOItemInsert + "VALUES (@finumber, @fpartno, @fpartrev, @fsono, @fllotreqd, @fautocreat, @fcas_bom, @fcas_rtg, @fcustpart, @fcustptrev, @fduedate, @fenumber, @fgroup, @fmeasure, @fmultiple, @fnextinum, @fnextrel, @fnunder, @fnover, @fordertype, @fprintmemo, @fprodcl, @fquantity, @fsource, @fdesc, @fdescmemo, @fac, @fclotext, @fstandpart, @sfac, @FcAltUM, @FnAltQty, @fcudrev)";

                        sQuerySORelInsert = "INSERT INTO sorels (fenumber, finumber, fpartno, fpartrev, frelease, fshptoaddr, fsono, fduedate, forderqty, fshpbefdue, fsplitshp, funetprice, flistaxabl, fdelivery, fpriority, fmasterrel, fbook, fnetprice, fcudrev) ";
                        sQuerySORelInsert = sQuerySORelInsert + "VALUES (@fenumber, @finumber, @fpartno, @fpartrev, @frelease, @fshptoaddr, @fsono, @fduedate, @forderqty, @fshpbefdue, @fsplitshp, @funetprice, @flistaxabl, @fdelivery, @fpriority, @fmasterrel, @fbook, @fnetprice, @fcudrev)";

                        sNextSONoQuery = "SELECT  fcnumber AS SalesOrderNo FROM  dbo.sysequ WHERE  (fcclass = 'SOMAST.FSONO')";

                        sNextSONoUpdateQuery = "UPDATE dbo.sysequ SET fcnumber = @newSONumber WHERE  (fcclass = 'SOMAST.FSONO')";

                        break;
                }


                if (so.IsNew)
                {
                    //Get Next Sales Order Number
                    var NextSONo = conn.QueryFirstOrDefault<SalesOrderNextNo>(sNextSONoQuery, transaction: trans);
                    so.SalesOrderNo = NextSONo.SalesOrderNo;

                    //Update the Next Sales Order Number
                    conn.Execute(sNextSONoUpdateQuery, new { newSONumber = NextSONo.NextSalesOrderNo }, transaction: trans);
                }


                //Create the new Sales Order
                conn.Execute(sQuerySOInsert, new
                {
                    fsono = so.SalesOrderNo.Truncate(6),
                    fcustno = so.CustomerNo.Truncate(6),
                    fcompany = so.CustomerName.Truncate(35),
                    fcity = so.SoldToCity.Truncate(20),
                    fcustpono = so.CustomerPONo.Truncate(20),
                    fackdate = so.AcknowledgedDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.AcknowledgedDate,
                    fcanc_dt = so.CancelledDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.CancelledDate,
                    fcontact = so.ContactName.Truncate(30),
                    fcountry = so.SoldToCountry.Truncate(25),
                    fcusrchr1 = so.UserDefinedString1.Truncate(20),
                    fcusrchr2 = so.UserDefinedstring2.Truncate(40),
                    fcusrchr3 = so.UserDefinedString3.Truncate(40),
                    fduedate = so.DueDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.DueDate,
                    fdusrdate1 = so.UserDefinedDate1 == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.UserDefinedDate1,
                    ffax = so.FaxNumber.Truncate(20),
                    ffob = so.FreightOnBoardPoint,
                    fnusrqty1 = so.UserDefinedQuantity1,
                    fnusrcur1 = so.UserDefinedCurrency1,
                    forderdate = so.OrderDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.OrderDate,
                    fpaytype = so.PaymentType,
                    fphone = so.PhoneNumber.Truncate(20),
                    fshipvia = so.ShipVia,
                    fshptoaddr = so.ShipToAddressKey,
                    fsocoord = so.SalesOrderCoordinator,
                    fsoldaddr = so.SoldToAddressKey,
                    fsoldby = so.SoldBy,
                    fstate = so.SoldToState.Truncate(20),
                    fstatus = so.Status,
                    fterm = so.Terms,
                    fterr = so.InternalSalesTerritory,
                    fzip = so.SoldToZip.Truncate(10),
                    //fncancchrge = so.CustomerSONumber,
                    fackmemo = so.SOAcknowledgeMemo,
                    fmstreet = so.SoldToStreet,
                    fmusrmemo1 = so.UserDefinedMemo1,
                    fpriority = so.Priority,
                    festimator = so.SalesOrderCoordinator,
                    fnextenum = so.NextExternalLineNo,
                    fnextinum = so.NextInternalLineNo,
                    fsorev = "00"
                    //CreatedDate = so.CreatedDate,
                    //ModifiedDate = so.ModifiedDate,
                    //fbilladdr = so.BillToAddressKey
                }, transaction: trans);

                foreach (SalesOrderItem line in so.SalesOrderLineItems)
                {
                    //Create the new Sales Order Line Item
                    conn.Execute(sQuerySOItemInsert, new
                    {
                        @finumber = line.InternalItemNo,
                        @fpartno = line.PartNo,
                        @fpartrev = line.PartRev,
                        @fsono = so.SalesOrderNo,
                        @fllotreqd = line.LotRequired,
                        @fautocreat = false,
                        @fcas_bom = false,
                        @fcas_rtg = false,
                        @fcustpart = line.CustomerPartNo,
                        @fcustptrev = line.CustomerPartRev == null ? "" : line.CustomerPartRev,
                        @fduedate = line.DueDateToShip == null ? DateTime.Parse("1900-01-01 00:00:00.000") : line.DueDateToShip,
                        @fenumber = line.ExternalItemNo,
                        @fgroup = line.PartGroup,
                        @fmeasure = line.UnitOfMeasure,
                        @fmultiple = line.IsBlanketRelease,
                        @fnextinum = 1,
                        @fnextrel = 1,
                        @fnunder = line.QuantityUnder,
                        @fnover = line.QuantityOver,
                        @fordertype = "Fix",
                        @fprintmemo = line.PrintMemo,
                        @fprodcl = line.PartClass,
                        @fquantity = line.Quantity,
                        @fsource = line.PartSource,
                        @fdesc = line.DescriptionShort,
                        @fdescmemo = line.DescriptionMemo == null ? "" : line.DescriptionMemo,
                        @fac = line.Facility == null || line.Facility == "" ? "Default" : line.Facility,
                        @fclotext = "S",
                        @fstandpart = true,
                        @sfac = line.Facility == null || line.Facility == "" ? "Default" : line.Facility,
                        @FcAltUM = line.UnitOfMeasure,
                        @FnAltQty = line.Quantity,
                        @fcudrev = line.PartRev

                    }, transaction: trans);

                    foreach (SalesOrderReleases rel in line.SalesOrderReleases)
                    {
                        //Create the new Sales Order Line Item
                        conn.Execute(sQuerySORelInsert, new
                        {
                            @fenumber = line.ExternalItemNo,
                            @finumber = line.InternalItemNo,
                            @fpartno = line.PartNo,
                            @fpartrev = line.PartRev,
                            @frelease = rel.IsMasterRelease ? "000" : rel.ReleaseNo,
                            @fshptoaddr = rel.ShipToAddressKeyR,
                            @fsono = so.SalesOrderNo,
                            @fduedate = rel.DueDateR == null ? DateTime.Parse("1900-01-01 00:00:00.000") : rel.DueDateR,
                            @forderqty = rel.QuantityR,
                            @fshpbefdue = rel.CanShipBeforeDue,
                            @fsplitshp = rel.AllowSplitShipments,
                            @funetprice = rel.UnitPrice,
                            @flistaxabl = rel.IsTaxable,
                            @fdelivery = rel.DeliveryNotes == null ? "" : rel.DeliveryNotes,
                            @fpriority = rel.PriorityR >= 1 && rel.PriorityR <= 9 ? rel.PriorityR : 4,
                            @fmasterrel = false,
                            @fbook = rel.QuantityR,
                            @fnetprice = rel.UnitPrice,
                            @fcudrev = line.PartRev

                        }, transaction: trans);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);

            }
        }

        private void insertSOExt(string location, SalesOrder so, IDbConnection conn, IDbTransaction trans)
        {
            try
            {
                string sQuerySOInsert = "";
                string sQuerySOItemInsert = "";
                string sQuerySORelInsert = "";

                // Generate queries based on location
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuerySOInsert = "";
                        break;
                    case "080":
                        //Need different query for 7.5

                        break;
                    default:

                        sQuerySOInsert = "INSERT INTO SOMAST_EXT (FKey_ID) VALuES (@FKey_ID) ";

                        sQuerySOItemInsert = "INSERT INTO SOITEM_EXT (FKey_ID, CPOLINE) VALUES (@FKey_ID, @CPOLINE) ";

                        sQuerySORelInsert = "INSERT INTO SORELS_EXT (FKey_ID) VALUES (@FKey_ID) ";

                        break;
                }


                //Create the new Sales Order EXT
                conn.Execute(sQuerySOInsert, new
                {
                    FKey_ID = so.SalesOrderId
                }, transaction: trans);

                foreach (SalesOrderItem line in so.SalesOrderLineItems)
                {
                    //Create the new Sales Order Line Item EXT
                    conn.Execute(sQuerySOItemInsert, new
                    {
                        FKey_ID = line.SalesOrderItemId,
                        CPOLINE = line.POLineNo

                    }, transaction: trans);

                    foreach (SalesOrderReleases rel in line.SalesOrderReleases)
                    {
                        //Create the new Sales Order Line Item
                        conn.Execute(sQuerySORelInsert, new
                        {
                            FKey_ID = rel.SalesOrderReleaseId

                        }, transaction: trans);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);

            }
        }

        private void updateSO(string location, SalesOrder so, IDbConnection conn, IDbTransaction trans)
        {
            try
            {
                string sQuerySOUpdate = "";
                string sQuerySOItemUpdate = "";
                string sQuerySORelUpdate = "";
                string sQuerySOItemEXTUpdate = "";

                // Generate queries based on location
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuerySOUpdate = "";
                        break;
                    case "080":
                        //Need different query for 7.5

                        break;
                    default:

                        sQuerySOUpdate = "UPDATE somast SET fcustpono = @fcustpono, fackdate = @fackdate, fcanc_dt = @fcanc_dt, fduedate = @fduedate, ffob = @ffob, fshipvia = @fshipvia, fshptoaddr = @fshptoaddr, fstatus = @fstatus, fterm = @fterm, fpriority = @fpriority, fnextenum = @fnextenum, fnextinum = @fnextinum, fsorev = @fsorev WHERE fsono = @fsono";

                        sQuerySOItemUpdate = "UPDATE soitem SET fpartno = @fpartno, fpartrev = @fpartrev, fcustpart = @fcustpart, fcustptrev = @fcustptrev, fduedate = @fduedate, fgroup = @fgroup, fmeasure = @fmeasure, fmultiple = @fmultiple, fnextinum = @fnextinum, fnextrel = @fnextrel, fordertype = @fordertype, fprodcl = @fprodcl, fquantity = @fquantity, fdesc = @fdesc, fdescmemo = @fdescmemo, FcAltUM = @FcAltUM, FnAltQty = @FnAltQty, fcudrev = @fcudrev WHERE fsono = @fsono AND finumber = @finumber ";

                        sQuerySOItemEXTUpdate = "UPDATE SOITEM_EXT SET CPOLine = @CPOLine WHERE FKey_ID = @FKey_ID";

                        sQuerySORelUpdate = "UPDATE sorels SET fpartno = @fpartno, fpartrev = @fpartrev, fshptoaddr = @fshptoaddr, fduedate = @fduedate, forderqty = @forderqty, fshpbefdue = @fshpbefdue, fsplitshp = @fsplitshp, funetprice = @funetprice, flistaxabl = @flistaxabl, fdelivery = @fdelivery, fpriority = @fpriority, fmasterrel = @fmasterrel, fbook = @fbook, fnetprice = @fnetprice, fcudrev = @fcudrev WHERE finumber = @finumber AND fsono = @fsono AND frelease = @frelease ";
                        break;
                }



                //Update the new Sales Order
                conn.Execute(sQuerySOUpdate, new
                {
                    fsono = so.SalesOrderNo,
                    fcustpono = so.CustomerPONo.Truncate(20),
                    fackdate = so.AcknowledgedDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.AcknowledgedDate,
                    fcanc_dt = so.CancelledDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.CancelledDate,
                    fduedate = so.DueDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.DueDate,
                    ffob = so.FreightOnBoardPoint,
                    fshipvia = so.ShipVia,
                    fshptoaddr = so.ShipToAddressKey,
                    fstatus = so.Status,
                    fterm = so.Terms,
                    //fncancchrge = so.CustomerSONumber,
                    fpriority = so.Priority,
                    fnextenum = so.NextExternalLineNo,
                    fnextinum = so.NextInternalLineNo,
                    fsorev = "00"
                    //CreatedDate = so.CreatedDate,
                    //ModifiedDate = so.ModifiedDate,
                    //fbilladdr = so.BillToAddressKey
                }, transaction: trans);

                foreach (SalesOrderItem line in so.SalesOrderLineItems)
                {
                    if (line.IsNewLine)
                    {
                        //Create the new Sales Order Line Item
                        conn.Execute(this.GetQuery(location, SQLQueryStatement.SOLineInsert), new
                        {
                            @finumber = line.InternalItemNo,
                            @fpartno = line.PartNo,
                            @fpartrev = line.PartRev,
                            @fsono = so.SalesOrderNo,
                            @fllotreqd = line.LotRequired,
                            @fautocreat = false,
                            @fcas_bom = false,
                            @fcas_rtg = false,
                            @fcustpart = line.CustomerPartNo,
                            @fcustptrev = line.CustomerPartRev == null ? "" : line.CustomerPartRev,
                            @fduedate = line.DueDateToShip == null ? DateTime.Parse("1900-01-01 00:00:00.000") : line.DueDateToShip,
                            @fenumber = line.ExternalItemNo,
                            @fgroup = line.PartGroup,
                            @fmeasure = line.UnitOfMeasure,
                            @fmultiple = line.IsBlanketRelease,
                            @fnextinum = 1,
                            @fnextrel = 1,
                            @fnunder = line.QuantityUnder,
                            @fnover = line.QuantityOver,
                            @fordertype = "Fix",
                            @fprintmemo = line.PrintMemo,
                            @fprodcl = line.PartClass,
                            @fquantity = line.Quantity,
                            @fsource = line.PartSource,
                            @fdesc = line.DescriptionShort,
                            @fdescmemo = line.DescriptionMemo == null ? "" : line.DescriptionMemo,
                            @fac = line.Facility == null || line.Facility == "" ? "Default" : line.Facility,
                            @fclotext = "S",
                            @fstandpart = true,
                            @sfac = line.Facility == null || line.Facility == "" ? "Default" : line.Facility,
                            @FcAltUM = line.UnitOfMeasure,
                            @FnAltQty = line.Quantity,
                            @fcudrev = line.PartRev

                        }, transaction: trans);

                    }
                    else
                    {
                        //Update the new Sales Order Line Item
                        conn.Execute(sQuerySOItemUpdate, new
                        {
                            @fsono = so.SalesOrderNo,
                            @finumber = line.InternalItemNo,
                            @fpartno = line.PartNo,
                            @fpartrev = line.PartRev,
                            @fcustpart = line.CustomerPartNo,
                            @fcustptrev = line.CustomerPartRev == null ? "" : line.CustomerPartRev,
                            @fduedate = line.DueDateToShip == null ? DateTime.Parse("1900-01-01 00:00:00.000") : line.DueDateToShip,
                            @fgroup = line.PartGroup,
                            @fmeasure = line.UnitOfMeasure,
                            @fmultiple = line.IsBlanketRelease,
                            @fnextinum = 1,
                            @fnextrel = 1,
                            @fordertype = "Fix",
                            @fprodcl = line.PartClass,
                            @fquantity = line.Quantity,
                            @fdesc = line.DescriptionShort,
                            @fdescmemo = line.DescriptionMemo == null ? "" : line.DescriptionMemo,
                            @FcAltUM = line.UnitOfMeasure,
                            @FnAltQty = line.Quantity,
                            @fcudrev = line.PartRev

                        }, transaction: trans);

                        conn.Execute(sQuerySOItemEXTUpdate, new
                        {
                            @FKey_ID = line.SalesOrderItemId,
                            @CPOLine = line.POLineNo
                        }, transaction: trans);
                    }


                    foreach (SalesOrderReleases rel in line.SalesOrderReleases)
                    {
                        if (rel.IsNewLine)
                        {
                            //Create the new Sales Order Line Item
                            conn.Execute(this.GetQuery(location, SQLQueryStatement.SORelInsert), new
                            {
                                @fenumber = line.ExternalItemNo,
                                @finumber = line.InternalItemNo,
                                @fpartno = line.PartNo,
                                @fpartrev = line.PartRev,
                                @frelease = rel.IsMasterRelease ? "000" : rel.ReleaseNo,
                                @fshptoaddr = rel.ShipToAddressKeyR,
                                @fsono = so.SalesOrderNo,
                                @fduedate = rel.DueDateR == null ? DateTime.Parse("1900-01-01 00:00:00.000") : rel.DueDateR,
                                @forderqty = rel.QuantityR,
                                @fshpbefdue = rel.CanShipBeforeDue,
                                @fsplitshp = rel.AllowSplitShipments,
                                @funetprice = rel.UnitPrice,
                                @flistaxabl = rel.IsTaxable,
                                @fdelivery = rel.DeliveryNotes == null ? "" : rel.DeliveryNotes,
                                @fpriority = rel.PriorityR >= 1 && rel.PriorityR <= 9 ? rel.PriorityR : 4,
                                @fmasterrel = false,
                                @fbook = rel.QuantityR,
                                @fnetprice = rel.UnitPrice,
                                @fcudrev = line.PartRev

                            }, transaction: trans);

                        }
                        else
                        {
                            //Update the new Sales Order Line Item
                            conn.Execute(sQuerySORelUpdate, new
                            {
                                @fsono = so.SalesOrderNo,
                                @finumber = line.InternalItemNo,
                                @frelease = rel.ReleaseNo,
                                @fpartno = line.PartNo,
                                @fpartrev = line.PartRev,
                                @fshptoaddr = rel.ShipToAddressKeyR,
                                @fduedate = rel.DueDateR == null ? DateTime.Parse("1900-01-01 00:00:00.000") : rel.DueDateR,
                                @forderqty = rel.QuantityR,
                                @fshpbefdue = rel.CanShipBeforeDue,
                                @fsplitshp = rel.AllowSplitShipments,
                                @funetprice = rel.UnitPrice,
                                @flistaxabl = rel.IsTaxable,
                                @fdelivery = rel.DeliveryNotes == null ? "" : rel.DeliveryNotes,
                                @fpriority = rel.PriorityR >= 1 && rel.PriorityR <= 9 ? rel.PriorityR : 4,
                                @fmasterrel = false,
                                @fbook = rel.QuantityR,
                                @fnetprice = rel.UnitPrice,
                                @fcudrev = line.PartRev

                            }, transaction: trans);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);

            }
        }

        private SalesOrder SelectSO(string location, SalesOrder so)
        {
            string sQuery = "";
            SalesOrder returnSO = null;

            // Retrieve shipping info
            switch (location)
            {
                case "072":
                    sQuery = "";
                    break;
                default:
                    sQuery = "SELECT        dbo.somast.identity_column AS SalesOrderId, dbo.somast.fsono AS SalesOrderNo, dbo.somast.fcustno AS CustomerNo, dbo.somast.fcompany AS CustomerName, dbo.somast.fcustpono AS CustomerPONo,                          dbo.somast.fackdate AS AcknowledgedDate, dbo.somast.fcanc_dt AS CancelledDate, dbo.somast.forderdate AS OrderDate, dbo.somast.fstatus AS Status, dbo.somast.fcity AS SoldToCity, dbo.somast.fcountry AS SoldToCountry,                          dbo.somast.fstate AS SoldToState, dbo.somast.fzip AS SoldToZip, dbo.somast.fcontact AS ContactName, dbo.somast.fduedate AS DueDate, dbo.somast.ffax AS FaxNumber, dbo.somast.fphone AS PhoneNumber,                          dbo.somast.ffob AS FreightOnBoardPoint, dbo.somast.fpaytype AS PaymentType, dbo.somast.fshipvia AS ShipVia, dbo.somast.fshptoaddr AS ShipToAddressKey, dbo.somast.fsoldaddr AS SoldToAddressKey,                          dbo.somast.fsocoord AS SalesOrderCoordinator, dbo.somast.fsoldby AS SoldBy, dbo.somast.fterm AS Terms, dbo.somast.fterr AS InternalSalesTerritory, dbo.somast.fackmemo AS SOAcknowledgeMemo,                          dbo.somast.fpriority AS Priority, dbo.soitem.identity_column AS SalesOrderItemId, dbo.soitem.finumber AS InternalItemNo, dbo.soitem.fpartno AS PartNo, dbo.soitem.fpartrev AS PartRev, dbo.soitem.fcustpart AS CustomerPartNo,                           dbo.soitem.fcustptrev AS CustomerPartRev, dbo.soitem.fduedate AS DueDateToShip, dbo.soitem.fquantity AS Quantity, dbo.soitem.fmeasure AS UnitOfMeasure, dbo.soitem.fdesc AS PartDesc, dbo.soitem.fgroup AS PartGroup,                          dbo.soitem.fclotext AS TextLoc, dbo.soitem.fllotreqd AS LotRequired, dbo.soitem.fenumber AS ExternalItemNo, dbo.soitem.fmultiple AS IsBlanketRelease, dbo.soitem.fnunder AS QuantityUnder,                          dbo.soitem.fnover AS QuantityOver, dbo.soitem.fprintmemo AS PrintMemo, dbo.soitem.fprodcl AS PartClass, dbo.soitem.fsource AS PartSource, dbo.soitem.fdescmemo AS DescriptionMemo, dbo.soitem.fac AS Facility,                          dbo.soitem.sfac AS SourceFacility, dbo.soitem.FnAltQty AS AlternateQuantity, dbo.soitem.FcAltUM AS AlternateUnitOfMeasure, dbo.sorels.identity_column AS SalesOrderReleaseId, dbo.sorels.frelease AS ReleaseNo,                          dbo.sorels.fshptoaddr AS ShipToAddressKeyR, dbo.sorels.fduedate AS DueDateR, dbo.sorels.fmasterrel AS IsMasterRelease, dbo.sorels.fnetprice AS NetPrice, dbo.sorels.forderqty AS QuantityR,                          dbo.sorels.fshpbefdue AS CanShipBeforeDue, dbo.sorels.fsplitshp AS AllowSplitShipments, dbo.sorels.funetprice AS UnitPrice, dbo.sorels.flistaxabl AS IsTaxable, dbo.sorels.fpriority AS PriorityR,                          dbo.sorels.fdelivery AS DeliveryNotes FROM            dbo.somast INNER JOIN                          dbo.soitem ON dbo.somast.fsono = dbo.soitem.fsono INNER JOIN                          dbo.sorels ON dbo.soitem.fsono = dbo.sorels.fsono AND dbo.soitem.finumber = dbo.sorels.finumber WHERE        (dbo.somast.fsono = @salesOrderNo)";
                    break;
            }


            using (var connection = Connection(location))
            {
                var soDictionary = new Dictionary<int, SalesOrder>();
                var soItemDictionary = new Dictionary<int, SalesOrderItem>();
                var soReleaseDictionary = new Dictionary<int, SalesOrderReleases>();


                var list = connection.Query<SalesOrder, SalesOrderItem, SalesOrderReleases, SalesOrder>(
                        sQuery,
                        (somain, soitem, sorelease) =>
                        {
                            SalesOrder soEntry;
                            SalesOrderItem soItemEntry;
                            SalesOrderReleases soReleaseEntry;

                            if (!soDictionary.TryGetValue(somain.SalesOrderId, out soEntry))
                            {
                                soEntry = somain;
                                soEntry.SalesOrderLineItems = new List<SalesOrderItem>();
                                soDictionary.Add(soEntry.SalesOrderId, soEntry);
                                soEntry.SalesOrderLineItems.Add(soitem);
                            }
                            else
                            {
                                if (!soEntry.SalesOrderLineItems.Exists(x => x.SalesOrderItemId == soitem.SalesOrderItemId))
                                {
                                    soEntry.SalesOrderLineItems.Add(soitem);
                                }
                            }


                            if (!soItemDictionary.TryGetValue(soitem.SalesOrderItemId, out soItemEntry))
                            {
                                soItemEntry = soitem;
                                soItemEntry.SalesOrderReleases = new List<SalesOrderReleases>();
                                soItemDictionary.Add(soitem.SalesOrderItemId, soItemEntry);
                                soItemEntry.SalesOrderReleases.Add(sorelease);
                            }
                            else
                            {
                                if (!soItemEntry.SalesOrderReleases.Exists(x => x.SalesOrderReleaseId == sorelease.SalesOrderReleaseId))
                                {
                                    soItemEntry.SalesOrderReleases.Add(sorelease);
                                }
                            }

                            returnSO = soEntry;
                            return returnSO;
                        },
                        param: new { salesOrderNo = so.SalesOrderNo },
                        splitOn: "SalesOrderItemId, SalesOrderReleaseId")
                    .Distinct()
                    .ToList();

                return returnSO;
            }

        }

        private void setCosts(string location, SalesOrder so, IDbConnection conn, IDbTransaction trans)
        {
            string sUpdateLineCosts = "";
            string sUpdateReleaseCosts = "";

            // Generate queries based on location
            switch (location)
            {
                case "072":
                    //Syteline Query
                    sUpdateLineCosts = "";
                    break;
                case "080":
                    //Need different query for 7.5
                    sUpdateLineCosts = "";
                    break;
                default:
                    sUpdateLineCosts = "UPDATE soitem SET flabact = @flabact, fmatlact = @fmatlact, fovhdact = @fovhdact, ftoolact = @ftoolact WHERE fsono = @fsono AND finumber = @finumber";
                    sUpdateReleaseCosts = "UPDATE sorels SET flabcost = @flabcost, fmatlcost = @fmatlcost, fovhdcost = @fovhdcost, ftoolcost = @ftoolcost WHERE fsono = @fsono AND finumber = @finumber AND frelease = @frelease";
                    break;
            }


            foreach (SalesOrderItem line in so.SalesOrderLineItems)
            {

                //Use the M2M Stored Procedure to determine line item costs
                using (var lineCosts = conn.QueryMultiple("SalesGetRecalcOrderItemCosts", new { @OrderNumber = so.SalesOrderNo, @OrderItemNumber = line.InternalItemNo, @UTCompFCostEst = "", @UTCompFCostRef = "", @UTCompFCostMType = "", @OrderType = "S", @UseStandardTransitCost = 1, @GetExtendedCosts = 0 }, commandType: CommandType.StoredProcedure, transaction: trans))
                {
                    var returnValue = lineCosts.Read<M2MResult>().First();
                    if (returnValue.ReturnValue == 0)
                    {
                        var costs = lineCosts.Read<M2MLineCosts>().FirstOrDefault();

                        //Update line item costs
                        conn.Execute(sUpdateLineCosts, new { flabact = costs.LaborActual, fmatlact = costs.MaterialActual, fovhdact = costs.OverheadActual, ftoolact = costs.ToolActual, fsono = so.SalesOrderNo, finumber = line.InternalItemNo }, transaction: trans);

                    }
                }

                foreach (SalesOrderReleases rel in line.SalesOrderReleases)
                {
                    //Use the M2M Stored Procedure to determine release costs
                    using (var relCosts = conn.QueryMultiple("SalesGetRecalcOrderReleaseCosts", new { @SalesOrderNumber = so.SalesOrderNo, @SalesOrderItemNumber = line.InternalItemNo, @SalesOrderReleaseNumber = rel.ReleaseNo }, commandType: CommandType.StoredProcedure, transaction: trans))
                    {
                        var costs = relCosts.Read<M2MRelCosts>().FirstOrDefault();

                        //Update release costs
                        conn.Execute(sUpdateReleaseCosts, new { flabcost = costs.LaborCost, fmatlcost = costs.MaterialCost, fovhdcost = costs.OverheadCost, ftoolcost = costs.ToolCost, fsono = so.SalesOrderNo, finumber = line.InternalItemNo, frelease = rel.ReleaseNo }, transaction: trans);

                    }
                }
            }


        }

        private void setCostsHardCodedTesting(string location)
        {
            //NOTE: This procedure was only used for testing when initially setting up the logic.

            IDbTransaction trans;

            using (var connection = Connection(location))
            {
                connection.Open();
                //Use the M2M Stored Procedure to determine line item costs
                using (var lineCosts = connection.QueryMultiple("SalesGetRecalcOrderItemCosts", new { @OrderNumber = "019736", @OrderItemNumber = "  2", @UTCompFCostEst = "", @UTCompFCostRef = "", @UTCompFCostMType = "", @OrderType = "S", @UseStandardTransitCost = 1, @GetExtendedCosts = 0 }, commandType: CommandType.StoredProcedure))
                {
                    var junk = lineCosts.Read().First();
                    var costs = lineCosts.Read<M2MLineCosts>();
                }


            }


        }

        private bool isValid(SalesOrder so)
        {
            bool retVal = true;

            //Check Sales Order
            if (so.AcknowledgedDate != null)
            {
                if (!IsValidDate(so.AcknowledgedDate))
                    throw (new Exception("Sales Order Acknowledge is not valid."));
            }
            else
            {
                so.AcknowledgedDate = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            if (so.CancelledDate != null)
            {
                if (!IsValidDate(so.CancelledDate))
                    throw (new Exception("Sales Order Cancelled is not valid."));
            }
            else
            {
                so.CancelledDate = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            if (so.CreatedDate != null)
            {
                if (!IsValidDate(so.CreatedDate))
                    throw (new Exception("Sales Order Created is not valid."));
            }
            else
            {
                so.CreatedDate = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            if (so.DueDate != null)
            {
                if (!IsValidDate(so.DueDate))
                    throw (new Exception("Sales Order Due Date is not valid."));
            }
            else
            {
                so.DueDate = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            if (so.ModifiedDate != null)
            {
                if (!IsValidDate(so.ModifiedDate))
                    throw (new Exception("Sales Order Modified is not valid."));
            }
            else
            {
                so.ModifiedDate = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            if (so.OrderDate != null)
            {
                if (!IsValidDate(so.OrderDate))
                    throw (new Exception("Sales Order Order Date is not valid."));
            }
            else
            {
                so.OrderDate = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            if (so.UserDefinedDate1 != null)
            {
                if (!IsValidDate(so.UserDefinedDate1))
                    throw (new Exception("Sales Order User Defined Date 1 is not valid."));
            }
            else
            {
                so.UserDefinedDate1 = DateTime.Parse("1900-01-01 00:00:00.000");
            }

            //Check Sales Order Items
            foreach (SalesOrderItem soitem in so.SalesOrderLineItems)
            {
                if (soitem.DueDateToShip != null)
                {
                    if (!IsValidDate(soitem.DueDateToShip))
                        throw (new Exception("Sales Order Item Due Date is not valid."));
                }
                else
                {
                    soitem.DueDateToShip = DateTime.Parse("1900-01-01 00:00:00.000");
                }


                //Check Sales Order Item Releases
                foreach (SalesOrderReleases sorel in soitem.SalesOrderReleases)
                {
                    if (sorel.DueDateR != null)
                    {
                        if (!IsValidDate(sorel.DueDateR))
                            throw (new Exception("Sales Order Item Release Due Date is not valid."));
                    }
                    else
                    {
                        sorel.DueDateR = DateTime.Parse("1900-01-01 00:00:00.000");
                    }
                }
            }

            return retVal;
        }

        private bool IsValidDate(DateTime? input)
        {
            return (input >= DateTime.Parse("1900-01-01 00:00:00.000") && input <= DateTime.Parse("2100-12-31 00:00:00.000"));
        }

        private enum SQLQueryStatement
        {
            SOInsert,
            SOUpdate,
            SOLineInsert,
            SOLineUpdate,
            SORelInsert,
            SORelUpdate
        }

        private string GetQuery(string siteCode, SQLQueryStatement queryType)
        {
            string retVal = "";

            // Generate queries based on location
            switch (siteCode)
            {
                case "072":
                    //Syteline Query
                    switch (queryType)
                    {
                        case SQLQueryStatement.SOInsert:
                            break;
                        case SQLQueryStatement.SOUpdate:
                            break;
                        case SQLQueryStatement.SOLineInsert:
                            break;
                        case SQLQueryStatement.SOLineUpdate:
                            break;
                        case SQLQueryStatement.SORelInsert:
                            break;
                        case SQLQueryStatement.SORelUpdate:
                            break;
                    }
                    break;
                case "080":
                    //Need different query for 7.5
                    switch (queryType)
                    {
                        case SQLQueryStatement.SOInsert:
                            retVal = "INSERT INTO somast (fsono, fcustno, fcompany, fcity, fcustpono, fackdate, fcanc_dt, fcontact, fcountry, fcusrchr1, fcusrchr2, fcusrchr3, fduedate, fdusrdate1, ffax, ffob, fnusrqty1, fnusrcur1, forderdate, fpaytype, fphone, fshipvia, fshptoaddr, fsocoord, fsoldaddr, fsoldby, fstate, fstatus, fterm, fterr, fzip, fncancchrge, fackmemo, fmstreet, fmusrmemo1, fpriority, CreatedDate, ModifiedDate, fbilladdr) ";
                            retVal = retVal + "VALUES (@fsono, @fcustno, @fcompany, @fcity, @fcustpono, @fackdate, @fcanc_dt, @fcontact, @fcountry, @fcusrchr1, @fcusrchr2, @fcusrchr3, @fduedate, @fdusrdate1, @ffax, @ffob, @fnusrqty1, @fnusrcur1, @forderdate, @fpaytype, @fphone, @fshipvia, @fshptoaddr, @fsocoord, @fsoldaddr, @fsoldby, @fstate, @fstatus, @fterm, @fterr, @fzip, @fncancchrge, @fackmemo, @fmstreet, @fmusrmemo1, @fpriority, @CreatedDate, @ModifiedDate, @fbilladdr)";

                            break;
                        case SQLQueryStatement.SOUpdate:
                            break;
                        case SQLQueryStatement.SOLineInsert:
                            break;
                        case SQLQueryStatement.SOLineUpdate:
                            break;
                        case SQLQueryStatement.SORelInsert:
                            break;
                        case SQLQueryStatement.SORelUpdate:
                            break;
                    }

                    break;
                default:
                    switch (queryType)
                    {
                        case SQLQueryStatement.SOInsert:
                            retVal = "INSERT INTO somast (fsono, fcustno, fcompany, fcity, fcustpono, fackdate, fcanc_dt, fcontact, fcountry, fcusrchr1, fcusrchr2, fcusrchr3, fduedate, fdusrdate1, ffax, ffob, fnusrqty1, fnusrcur1, forderdate, fpaytype, fphone, fshipvia, fshptoaddr, fsocoord, fsoldaddr, fsoldby, fstate, fstatus, fterm, fterr, fzip, fackmemo, fmstreet, fmusrmemo1, fpriority, festimator, fnextenum, fnextinum, fsorev) ";
                            retVal = retVal + "VALUES (@fsono, @fcustno, @fcompany, @fcity, @fcustpono, @fackdate, @fcanc_dt, @fcontact, @fcountry, @fcusrchr1, @fcusrchr2, @fcusrchr3, @fduedate, @fdusrdate1, @ffax, @ffob, @fnusrqty1, @fnusrcur1, @forderdate, @fpaytype, @fphone, @fshipvia, @fshptoaddr, @fsocoord, @fsoldaddr, @fsoldby, @fstate, @fstatus, @fterm, @fterr, @fzip, @fackmemo, @fmstreet, @fmusrmemo1, @fpriority, @festimator, @fnextenum, @fnextinum, @fsorev)";

                            break;
                        case SQLQueryStatement.SOUpdate:
                            break;
                        case SQLQueryStatement.SOLineInsert:
                            retVal = "INSERT INTO soitem (finumber, fpartno, fpartrev, fsono, fllotreqd, fautocreat, fcas_bom, fcas_rtg, fcustpart, fcustptrev, fduedate, fenumber, fgroup, fmeasure, fmultiple, fnextinum, fnextrel, fnunder, fnover, fordertype, fprintmemo, fprodcl, fquantity, fsource, fdesc, fdescmemo, fac, fclotext, fstandpart, sfac, FcAltUM, FnAltQty, fcudrev) ";
                            retVal = retVal + "VALUES (@finumber, @fpartno, @fpartrev, @fsono, @fllotreqd, @fautocreat, @fcas_bom, @fcas_rtg, @fcustpart, @fcustptrev, @fduedate, @fenumber, @fgroup, @fmeasure, @fmultiple, @fnextinum, @fnextrel, @fnunder, @fnover, @fordertype, @fprintmemo, @fprodcl, @fquantity, @fsource, @fdesc, @fdescmemo, @fac, @fclotext, @fstandpart, @sfac, @FcAltUM, @FnAltQty, @fcudrev)";

                            break;
                        case SQLQueryStatement.SOLineUpdate:
                            break;
                        case SQLQueryStatement.SORelInsert:
                            retVal = "INSERT INTO sorels (fenumber, finumber, fpartno, fpartrev, frelease, fshptoaddr, fsono, fduedate, forderqty, fshpbefdue, fsplitshp, funetprice, flistaxabl, fdelivery, fpriority, fmasterrel, fbook, fnetprice, fcudrev) ";
                            retVal = retVal + "VALUES (@fenumber, @finumber, @fpartno, @fpartrev, @frelease, @fshptoaddr, @fsono, @fduedate, @forderqty, @fshpbefdue, @fsplitshp, @funetprice, @flistaxabl, @fdelivery, @fpriority, @fmasterrel, @fbook, @fnetprice, @fcudrev)";

                            break;
                        case SQLQueryStatement.SORelUpdate:
                            break;
                    }
                    break;
            }

            return retVal;
        }
    }
}

