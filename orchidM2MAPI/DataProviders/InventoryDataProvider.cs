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
    public class InventoryDataProvider : IInventoryDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public InventoryDataProvider(IConfigurationRoot config, ILoggerFactory logger)
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

        public async Task<List<Inventory>> GetInventoryOnHand(string location, string PartNo)
        {
            try
            {
                string sQuery = "";

                // Retrieve inventory on hand
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "SELECT        TOP (100) PERCENT dbo.job.job, dbo.job.suffix, dbo.jobroute.oper_num, dbo.job.item, dbo.jobroute.wc, dbo.wc.description FROM            dbo.jobroute INNER JOIN                          dbo.job ON dbo.jobroute.job = dbo.job.job AND dbo.jobroute.suffix = dbo.job.suffix INNER JOIN                          dbo.wc ON dbo.jobroute.wc = dbo.wc.wc WHERE        (dbo.job.job = @jobNo) AND (dbo.job.suffix = 0001) ORDER BY dbo.job.suffix, dbo.job.job_date DESC, dbo.jobroute.oper_num";
                        break;
                    default:
                        sQuery = "SELECT  fpartno AS PartNo, fpartrev AS PartRev, fbinno AS BinNo, flocation AS Location, flot AS LotNumber, fonhand AS Quantity, identity_column AS InventoryId, fac AS Facility FROM dbo.inonhd WHERE (fpartno = @partNo)";
                        break;
                }

                using (var connection = Connection(location))
                {
                    var list = connection.Query<Inventory>(
                        sQuery,
                        param: new { partNo = PartNo })
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

        public async Task<List<Inventory>> GetInventoryShipped(string location, string PartNo)
        {
            try
            {
                string sQuery = "";

                // Retrieve inventory on hand
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "SELECT        TOP (100) PERCENT dbo.job.job, dbo.job.suffix, dbo.jobroute.oper_num, dbo.job.item, dbo.jobroute.wc, dbo.wc.description FROM            dbo.jobroute INNER JOIN                          dbo.job ON dbo.jobroute.job = dbo.job.job AND dbo.jobroute.suffix = dbo.job.suffix INNER JOIN                          dbo.wc ON dbo.jobroute.wc = dbo.wc.wc WHERE        (dbo.job.job = @jobNo) AND (dbo.job.suffix = 0001) ORDER BY dbo.job.suffix, dbo.job.job_date DESC, dbo.jobroute.oper_num";
                        break;
                    default:
                        sQuery = "SELECT        dbo.shitem.fpartno AS PartNo, dbo.shitem.frev AS PartRev, dbo.shitem.fac AS Facility, dbo.shlotc.fclot AS LotNumber, dbo.shlotc.fnlotqty AS Quantity, dbo.shmast.fshipno AS ShipperNo, dbo.shmast.fshipdate AS ShipDate FROM            dbo.shmast INNER JOIN                          dbo.shitem ON dbo.shmast.fshipno = dbo.shitem.fshipno LEFT OUTER JOIN                          dbo.shlotc ON dbo.shitem.fitemno = dbo.shlotc.fcitemno AND dbo.shitem.fshipno = dbo.shlotc.fcshipno WHERE        (dbo.shitem.fpartno = @partNo)";
                        break;
                }

                using (var connection = Connection(location))
                {
                    var list = connection.Query<Inventory>(
                        sQuery,
                        param: new { partNo = PartNo })
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


        public async Task<List<Inventory>> GetInventoryJobs(string location, string PartNo)
        {
            try
            {
                string sQuery = "";

                // Retrieve inventory on hand
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "SELECT        TOP (100) PERCENT dbo.job.job, dbo.job.suffix, dbo.jobroute.oper_num, dbo.job.item, dbo.jobroute.wc, dbo.wc.description FROM            dbo.jobroute INNER JOIN                          dbo.job ON dbo.jobroute.job = dbo.job.job AND dbo.jobroute.suffix = dbo.job.suffix INNER JOIN                          dbo.wc ON dbo.jobroute.wc = dbo.wc.wc WHERE        (dbo.job.job = @jobNo) AND (dbo.job.suffix = 0001) ORDER BY dbo.job.suffix, dbo.job.job_date DESC, dbo.jobroute.oper_num";
                        break;
                    default:
                        sQuery = "SELECT        dbo.jomast.fpartno AS PartNo, dbo.jomast.fpartrev AS PartRev, dbo.jomast.fac AS Facility, dbo.jomast.fquantity - ISNULL(Scrap.TotalScrap, 0) AS Quantity, dbo.jomast.fstatus AS JobStatus, dbo.jomast.fddue_date AS JobDueDate,                           dbo.jomast.fjobno AS JobNo, JobLots.fclot AS LotNumber, JobLots.fnquantity AS LotQuantity FROM            dbo.jomast LEFT OUTER JOIN                              (SELECT        fcdoc, fcpartno, fcpartrev, fclot, fnquantity, fddate, fcmeasure, identity_column, fac                                FROM            dbo.qalotc                                WHERE        (fctype = 'J') AND (fcuseindoc = '')) AS JobLots ON dbo.jomast.fjobno = JobLots.fcdoc LEFT OUTER JOIN                              (SELECT        fjobno, SUM(fquantity) AS TotalScrap                                FROM            dbo.qajors                                WHERE        (ftype = 'S')                                GROUP BY fjobno) AS Scrap ON dbo.jomast.fjobno = Scrap.fjobno WHERE        (dbo.jomast.fpartno = @partno) AND (dbo.jomast.fstatus NOT IN ('CANCELLED', 'CLOSED'))";
                        break;
                }

                using (var connection = Connection(location))
                {
                    var list = connection.Query<Inventory>(
                        sQuery,
                        param: new { partNo = PartNo })
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

