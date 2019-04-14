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
            try
            {
                string sQuerySOInsert = "";
                string sQuerySOItemInsert = "";
                string sQuerySORelInsert = "";
                string sNextSONoQuery = "";
                string sNextSONoUpdateQuery = "";
                IDbTransaction trans;

                // Generate queries based on location
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuerySOInsert = "";
                        break;
                    case "080":
                        //Need different query for 7.5
                        if (so.IsNew)
                        {
                            sQuerySOInsert = "INSERT INTO somast (fsono, fcustno, fcompany, fcity, fcustpono, fackdate, fcanc_dt, fcontact, fcountry, fcusrchr1, fcusrchr2, fcusrchr3, fduedate, fdusrdate1, ffax, ffob, fnusrqty1, fnusrcur1, forderdate, fpaytype, fphone, fshipvia, fshptoaddr, fsocoord, fsoldaddr, fsoldby, fstate, fstatus, fterm, fterr, fzip, fncancchrge, fackmemo, fmstreet, fmusrmemo1, fpriority, CreatedDate, ModifiedDate, fbilladdr) ";
                            sQuerySOInsert = sQuerySOInsert + "VALUES (@fsono, @fcustno, @fcompany, @fcity, @fcustpono, @fackdate, @fcanc_dt, @fcontact, @fcountry, @fcusrchr1, @fcusrchr2, @fcusrchr3, @fduedate, @fdusrdate1, @ffax, @ffob, @fnusrqty1, @fnusrcur1, @forderdate, @fpaytype, @fphone, @fshipvia, @fshptoaddr, @fsocoord, @fsoldaddr, @fsoldby, @fstate, @fstatus, @fterm, @fterr, @fzip, @fncancchrge, @fackmemo, @fmstreet, @fmusrmemo1, @fpriority, @CreatedDate, @ModifiedDate, @fbilladdr)";

                            sNextSONoQuery = "SELECT  fcnumber AS SalesOrderNo FROM  dbo.sysequ WHERE  (fcclass = 'SOMAST.FSONO')";

                            sNextSONoUpdateQuery = "UPDATE dbo.sysequ SET fcnumber = @newSONumber WHERE  (fcclass = 'SOMAST.FSONO')";
                        }
                        else
                        {

                        }
                        break;
                    default:
                        if (so.IsNew)
                        {
                            sQuerySOInsert = "INSERT INTO somast (fsono, fcustno, fcompany, fcity, fcustpono, fackdate, fcanc_dt, fcontact, fcountry, fcusrchr1, fcusrchr2, fcusrchr3, fduedate, fdusrdate1, ffax, ffob, fnusrqty1, fnusrcur1, forderdate, fpaytype, fphone, fshipvia, fshptoaddr, fsocoord, fsoldaddr, fsoldby, fstate, fstatus, fterm, fterr, fzip, fackmemo, fmstreet, fmusrmemo1, fpriority) ";
                            sQuerySOInsert = sQuerySOInsert + "VALUES (@fsono, @fcustno, @fcompany, @fcity, @fcustpono, @fackdate, @fcanc_dt, @fcontact, @fcountry, @fcusrchr1, @fcusrchr2, @fcusrchr3, @fduedate, @fdusrdate1, @ffax, @ffob, @fnusrqty1, @fnusrcur1, @forderdate, @fpaytype, @fphone, @fshipvia, @fshptoaddr, @fsocoord, @fsoldaddr, @fsoldby, @fstate, @fstatus, @fterm, @fterr, @fzip, @fackmemo, @fmstreet, @fmusrmemo1, @fpriority)";

                            sQuerySOItemInsert = "INSERT INTO soitem (finumber, fpartno, fpartrev, fsono, fllotreqd, fautocreat, fcas_bom, fcas_rtg, fcustpart, fcustptrev, fduedate, fenumber, fgroup, fmeasure, fmultiple, fnextinum, fnextrel, fnunder, fnover, fordertype, fprintmemo, fprodcl, fquantity, fsource, fdesc, fdescmemo, fac) ";
                            sQuerySOItemInsert = sQuerySOItemInsert + "VALUES (@finumber, @fpartno, @fpartrev, @fsono, @fllotreqd, @fautocreat, @fcas_bom, @fcas_rtg, @fcustpart, @fcustptrev, @fduedate, @fenumber, @fgroup, @fmeasure, @fmultiple, @fnextinum, @fnextrel, @fnunder, @fnover, @fordertype, @fprintmemo, @fprodcl, @fquantity, @fsource, @fdesc, @fdescmemo, @fac)";

                            sQuerySORelInsert = "INSERT INTO sorels (fenumber, finumber, fpartno, fpartrev, frelease, fshptoaddr, fsono, fduedate, forderqty, fshpbefdue, fsplitshp, funetprice, flistaxabl, fdelivery, fpriority, fmasterrel) ";
                            sQuerySORelInsert = sQuerySORelInsert + "VALUES (@fenumber, @finumber, @fpartno, @fpartrev, @frelease, @fshptoaddr, @fsono, @fduedate, @forderqty, @fshpbefdue, @fsplitshp, @funetprice, @flistaxabl, @fdelivery, @fpriority, @fmasterrel)";

                            sNextSONoQuery = "SELECT  fcnumber AS SalesOrderNo FROM  dbo.sysequ WHERE  (fcclass = 'SOMAST.FSONO')";

                            sNextSONoUpdateQuery = "UPDATE dbo.sysequ SET fcnumber = @newSONumber WHERE  (fcclass = 'SOMAST.FSONO')";
                        }
                        else
                        {

                        }
                        break;
                }


                using (var connection = Connection(location))
                {
                    connection.Open();
                    using (trans = connection.BeginTransaction())
                    {
                        if (so.IsNew)
                        {
                            //Get Next Sales Order Number
                            var NextSONo = connection.QueryFirstOrDefault<SalesOrderNextNo>(sNextSONoQuery, transaction: trans);
                            so.SalesOrderNo = NextSONo.SalesOrderNo;

                            //Update the Next Sales Order Number
                            connection.Execute(sNextSONoUpdateQuery, new { newSONumber = NextSONo.NextSalesOrderNo }, transaction: trans);
                        }


                        //Create the new Sales Order
                        connection.Execute(sQuerySOInsert, new
                        {
                            fsono = so.SalesOrderNo,
                            fcustno = so.CustomerNo,
                            fcompany = so.CustomerName,
                            fcity = so.SoldToCity,
                            fcustpono = so.CustomerPONo,
                            fackdate = so.AcknowledgedDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.AcknowledgedDate,
                            fcanc_dt = so.CancelledDate == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.CancelledDate,
                            fcontact = so.ContactName,
                            fcountry = so.SoldToCountry,
                            fcusrchr1 = so.UserDefinedString1,
                            fcusrchr2 = so.UserDefinedstring2,
                            fcusrchr3 = so.UserDefinedString3,
                            fduedate = so.DueDate,
                            fdusrdate1 = so.UserDefinedDate1 == null ? DateTime.Parse("1900-01-01 00:00:00.000") : so.UserDefinedDate1,
                            ffax = so.FaxNumber,
                            ffob = so.FreightOnBoardPoint,
                            fnusrqty1 = so.UserDefinedQuantity1,
                            fnusrcur1 = so.UserDefinedCurrency1,
                            forderdate = so.OrderDate,
                            fpaytype = so.PaymentType,
                            fphone = so.PhoneNumber,
                            fshipvia = so.ShipVia,
                            fshptoaddr = so.ShipToAddressKey,
                            fsocoord = so.SalesOrderCoordinator,
                            fsoldaddr = so.SoldToAddressKey,
                            fsoldby = so.SoldBy,
                            fstate = so.SoldToState,
                            fstatus = so.Status,
                            fterm = so.Terms,
                            fterr = so.InternalSalesTerritory,
                            fzip = so.SoldToZip,
                            //fncancchrge = so.CustomerSONumber,
                            fackmemo = so.SOAcknowledgeMemo,
                            fmstreet = so.SoldToStreet,
                            fmusrmemo1 = so.UserDefinedMemo1,
                            fpriority = so.Priority
                            //CreatedDate = so.CreatedDate,
                            //ModifiedDate = so.ModifiedDate,
                            //fbilladdr = so.BillToAddressKey
                        }, transaction: trans);

                        foreach (SalesOrderItem line in so.Items)
                        {
                            //Create the new Sales Order Line Item
                            connection.Execute(sQuerySOItemInsert, new
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
                                @fduedate = line.DueDateToShip,
                                @fenumber = line.ExternalItemNo,
                                @fgroup = line.PartGroup,
                                @fmeasure = line.UnitOfMeasure,
                                @fmultiple = line.IsBlanketRelease,
                                @fnextinum = 0,
                                @fnextrel = 0,
                                @fnunder = line.QuantityUnder,
                                @fnover = line.QuantityOver,
                                @fordertype = "",
                                @fprintmemo = line.PrintMemo,
                                @fprodcl = line.PartClass,
                                @fquantity = line.Quantity,
                                @fsource = line.PartSource,
                                @fdesc = line.DescriptionShort,
                                @fdescmemo = line.DescriptionMemo == null ? "" : line.DescriptionMemo,
                                @fac = line.Facility == null || line.Facility == "" ? "Default" : line.Facility

                            }, transaction: trans);

                            foreach (SalesOrderReleases rel in line.Releases)
                            {
                                //Create the new Sales Order Line Item
                                connection.Execute(sQuerySORelInsert, new
                                {
                                    @fenumber = line.ExternalItemNo,
                                    @finumber = line.InternalItemNo,
                                    @fpartno = line.PartNo,
                                    @fpartrev = line.PartRev,
                                    @frelease = rel.IsMasterRelease ? "000" : rel.ReleaseNo,
                                    @fshptoaddr = rel.ShipToAddressKey,
                                    @fsono = so.SalesOrderNo,
                                    @fduedate = rel.DueDate,
                                    @forderqty = rel.Quantity,
                                    @fshpbefdue = rel.CanShipBeforeDue,
                                    @fsplitshp = rel.AllowSplitShipments,
                                    @funetprice = rel.UnitPrice,
                                    @flistaxabl = rel.IsTaxable,
                                    @fdelivery = rel.DeliveryNotes == null ? "" : rel.DeliveryNotes,
                                    @fpriority = rel.Priority >= 1 && rel.Priority <= 9 ? rel.Priority : 4,
                                    @fmasterrel = false

                                }, transaction: trans);
                            }
                        }

                        try
                        {
                            trans.Commit();
                            connection.Close();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }


                    return so;

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

