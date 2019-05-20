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
    public class JobDataProvider : IJobDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public JobDataProvider(IConfigurationRoot config, ILoggerFactory logger)
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

        public async Task<Job> GetJob(string location, string JobNo)
        {
            try
            {
                string sQuery = "";

                // Retrieve shipping info
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "SELECT        TOP (100) PERCENT dbo.job.job, dbo.job.suffix, dbo.jobroute.oper_num, dbo.job.item, dbo.jobroute.wc, dbo.wc.description FROM            dbo.jobroute INNER JOIN                          dbo.job ON dbo.jobroute.job = dbo.job.job AND dbo.jobroute.suffix = dbo.job.suffix INNER JOIN                          dbo.wc ON dbo.jobroute.wc = dbo.wc.wc WHERE        (dbo.job.job = @jobNo) AND (dbo.job.suffix = 0001) ORDER BY dbo.job.suffix, dbo.job.job_date DESC, dbo.jobroute.oper_num";
                        break;
                    case "010":
                        sQuery = "SELECT        RTRIM(ISNULL(dbo.JOMAST_EXT.LASRETCH, '')) AS LaserEtch, dbo.jomast.identity_column AS JobId, dbo.jomast.fjobno AS JobNo, RTRIM(dbo.jomast.fpartno) AS PartNo, RTRIM(dbo.jomast.fpartrev) AS PartRev, dbo.jomast.ftype AS JobType, RTRIM(dbo.jomast.fstatus) AS JobStatus,                          dbo.jomast.fddue_date AS JobDueDate, dbo.jomast.fquantity AS JobQuantity, dbo.jomast.fjob_mem AS JobComments, dbo.joitem.fdescmemo AS JobMemo, RTRIM(SUBSTRING(dbo.joitem.fdesc, 1, 1000)) AS PartDesc,                          dbo.jodrtg.identity_column AS JobRouteStepId, dbo.jodrtg.foperno AS OperationNo, RTRIM(dbo.jodrtg.fpro_id) AS WorkCenterNo, RTRIM(dbo.inwork.fcpro_name) AS WorkCenterName,                          dbo.jodrtg.fnqty_comp AS QuantityCompleted, dbo.jodrtg.fopermemo AS OperationMemo, dbo.jodrtg.fsetuptime AS EstSetupTime, dbo.jodrtg.fuprodtime AS EstProductionTimePerUnit, dbo.jodrtg.fstrtdate AS StartDate,                          dbo.jodrtg.fnsh_date AS FinishDate  FROM            dbo.JOMAST_EXT RIGHT OUTER JOIN                          dbo.joitem INNER JOIN                          dbo.jomast ON dbo.joitem.fjobno = dbo.jomast.fjobno AND dbo.joitem.fpartno = dbo.jomast.fpartno AND dbo.joitem.fpartrev = dbo.jomast.fpartrev ON dbo.JOMAST_EXT.FKey_ID = dbo.jomast.identity_column LEFT OUTER JOIN                          dbo.jodrtg ON dbo.jomast.fjobno = dbo.jodrtg.fjobno LEFT OUTER JOIN                          dbo.inwork ON dbo.jodrtg.fpro_id = dbo.inwork.fcpro_id WHERE        (dbo.jomast.fjobno = @jobNo)";
                        break;
                    default:
                        sQuery = "SELECT        dbo.jomast.identity_column AS JobId, dbo.jomast.fjobno AS JobNo, RTRIM(dbo.jomast.fpartno) AS PartNo, RTRIM(dbo.jomast.fpartrev) AS PartRev, dbo.jomast.ftype AS JobType, RTRIM(dbo.jomast.fstatus) AS JobStatus,                          dbo.jomast.fddue_date AS JobDueDate, dbo.jomast.fquantity AS JobQuantity, dbo.jomast.fjob_mem AS JobComments, dbo.joitem.fdescmemo AS JobMemo, RTRIM(SUBSTRING(dbo.joitem.fdesc, 1, 1000)) AS PartDesc,                          dbo.jodrtg.identity_column AS JobRouteStepId, dbo.jodrtg.foperno AS OperationNo, RTRIM(dbo.jodrtg.fpro_id) AS WorkCenterNo, RTRIM(dbo.inwork.fcpro_name) AS WorkCenterName, dbo.jodrtg.fnqty_comp AS QuantityCompleted,                          dbo.jodrtg.fopermemo AS OperationMemo, dbo.jodrtg.fsetuptime AS EstSetupTime, dbo.jodrtg.fuprodtime AS EstProductionTimePerUnit, fstrtdate AS StartDate, fnsh_date AS FinishDate FROM            dbo.jodrtg RIGHT OUTER JOIN                          dbo.joitem INNER JOIN                          dbo.jomast ON dbo.joitem.fjobno = dbo.jomast.fjobno AND dbo.joitem.fpartno = dbo.jomast.fpartno AND dbo.joitem.fpartrev = dbo.jomast.fpartrev ON dbo.jodrtg.fjobno = dbo.jomast.fjobno LEFT OUTER JOIN                          dbo.inwork ON dbo.jodrtg.fpro_id = dbo.inwork.fcpro_id WHERE        (dbo.jomast.fjobno = @jobNo)";
                        break;
                }


                using (var connection = Connection(location))
                {
                    var jobDictionary = new Dictionary<int, Job>();


                    var list = connection.Query<Job, JobRouteStep, Job>(
                        sQuery,
                        (job, jobroutestep) =>
                        {
                            Job jobEntry;

                            if (!jobDictionary.TryGetValue(job.JobId, out jobEntry))
                            {
                                jobEntry = job;
                                jobEntry.Routing = new List<JobRouteStep>();
                                jobDictionary.Add(jobEntry.JobId, jobEntry);
                            }
                            jobEntry.Routing.Add(jobroutestep);


                            return jobEntry;
                        },
                        param: new { jobNo = JobNo },
                        splitOn: "JobRouteStepId")
                    .Distinct()
                    .ToList();

                    return jobDictionary.FirstOrDefault().Value;

                }
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

        public async Task<Job> GetJobLots(string location, string JobNo)
        {
            try
            {
                string sQuery = "";
                string newJobNo = "";
                short newSuffix = 0;

                // Retrieve shipping info
                switch (location)
                {
                    case "076":
                    case "072":
                        //Syteline Query, may send a suffix after the hypen, if so split and change the where
                        newJobNo = JobNo.Split("-")[0];
                        newSuffix = JobNo.Split("-").Count() > 1 ? short.Parse(JobNo.Split("-")[1]) : short.Parse("0");

                        //sQuery = "SELECT        0 AS JobId, dbo.job.job + '-' + CAST(dbo.job.suffix AS NVARCHAR(5)) AS JobNo, dbo.lot.item AS PartNo, dbo.item.revision AS PartRev, '' AS JobType, dbo.job.ord_num AS SalesOrderNo, dbo.job.cust_num AS CustomerNo,                          dbo.job.stat AS JobStatus, dbo.coitem.due_date AS JobDueDate, dbo.job.qty_released AS JobQuantity, 0 AS JobLotId, dbo.lot.lot AS LotNo, dbo.preassigned_lot.qty_received AS LotQuantity, 'EA' AS LotQtyUnitOfMeasure FROM            dbo.lot INNER JOIN                          dbo.item ON dbo.lot.item = dbo.item.item INNER JOIN                          dbo.preassigned_lot ON dbo.lot.lot = dbo.preassigned_lot.lot INNER JOIN                          dbo.job ON dbo.preassigned_lot.ref_num = dbo.job.job AND dbo.preassigned_lot.ref_line_suf = dbo.job.suffix LEFT OUTER JOIN                          dbo.coitem ON dbo.job.ord_line = dbo.coitem.co_line AND dbo.job.ord_num = dbo.coitem.co_num WHERE        (dbo.preassigned_lot.ref_type = N'J') AND (dbo.job.job = @jobNo) AND (dbo.job.suffix = @suffixNo)";
                        sQuery = "SELECT        TOP (100) PERCENT 0 AS JobId, dbo.job.job + '-' + CAST(dbo.job.suffix AS NVARCHAR(5)) AS JobNo, dbo.job.item AS PartNo, dbo.item.revision AS PartRev, '' AS JobType, dbo.job.ord_num AS SalesOrderNo,                          RTRIM(LTRIM(dbo.job.cust_num)) AS CustomerNo, dbo.job.stat AS JobStatus, dbo.job.job_date AS JobDueDate, dbo.job.qty_released AS JobQuantity, 0 AS JobLotId, dbo.preassigned_lot.lot AS LotNo,                          dbo.preassigned_lot.qty_received AS LotQuantity, 'EA' AS LotQtyUnitOfMeasure FROM            dbo.item INNER JOIN                          dbo.job ON dbo.item.item = dbo.job.item INNER JOIN                          dbo.preassigned_lot ON dbo.item.item = dbo.preassigned_lot.item AND dbo.job.job = dbo.preassigned_lot.ref_num AND dbo.job.suffix = dbo.preassigned_lot.ref_line_suf WHERE        (dbo.job.job = @jobNo) AND (dbo.job.suffix = @suffixNo) AND (dbo.preassigned_lot.ref_type = N'J')";
                        break;
                    case "032":
                        newJobNo = JobNo;

                        //Detroit - They use the suffix 0000, 0001, etc of job numbers, but it appears only the 0000 has the lot number
                        //           therefore I've changed the from clause to get lot numbers to only look at the first 5 digits of the job no
                        sQuery = "SELECT        dbo.jomast.identity_column AS JobId, dbo.jomast.fjobno AS JobNo, RTRIM(dbo.jomast.fpartno) AS PartNo, RTRIM(dbo.jomast.fpartrev) AS PartRev, dbo.jomast.ftype AS JobType, dbo.jomast.fsono AS SalesOrderNo,                          dbo.somast.fcustno AS CustomerNo, RTRIM(dbo.jomast.fstatus) AS JobStatus, dbo.jomast.fddue_date AS JobDueDate, dbo.jomast.fquantity AS JobQuantity, JobLots.identity_column AS JobLotId, RTRIM(JobLots.fclot) AS LotNo,                          JobLots.fnquantity AS LotQuantity, RTRIM(JobLots.fcmeasure) AS LotQtyUnitOfMeasure FROM            dbo.jomast LEFT OUTER JOIN                          dbo.somast ON dbo.jomast.fsono = dbo.somast.fsono LEFT OUTER JOIN                              (SELECT        fcdoc, fcpartno, fcpartrev, fclot, fnquantity, fddate, fcmeasure, identity_column, fac, fcuseindoc                                FROM            dbo.qalotc                                WHERE        (fctype = 'J')) AS JobLots ON LEFT(dbo.jomast.fjobno, 5) = LEFT(JobLots.fcdoc, 5) AND (LEFT(dbo.jomast.fjobno, 5) = LEFT(JobLots.fcuseindoc, 5) OR                          JobLots.fcuseindoc = '') WHERE        (dbo.jomast.fjobno = @jobNo)";
                        break;
                    default:
                        newJobNo = JobNo;

                        sQuery = "SELECT        dbo.jomast.identity_column AS JobId, dbo.jomast.fjobno AS JobNo, RTRIM(dbo.jomast.fpartno) AS PartNo, RTRIM(dbo.jomast.fpartrev) AS PartRev, dbo.jomast.ftype AS JobType, dbo.jomast.fsono AS SalesOrderNo,                          dbo.somast.fcustno AS CustomerNo, RTRIM(dbo.jomast.fstatus) AS JobStatus, dbo.jomast.fddue_date AS JobDueDate, dbo.jomast.fquantity AS JobQuantity, JobLots.identity_column AS JobLotId, RTRIM(JobLots.fclot) AS LotNo,                          JobLots.fnquantity AS LotQuantity, RTRIM(JobLots.fcmeasure) AS LotQtyUnitOfMeasure FROM            dbo.jomast LEFT OUTER JOIN                          dbo.somast ON dbo.jomast.fsono = dbo.somast.fsono LEFT OUTER JOIN                              (SELECT        fcdoc, fcpartno, fcpartrev, fclot, fnquantity, fddate, fcmeasure, identity_column, fac                                FROM            dbo.qalotc                                WHERE        (fctype = 'J') AND (fcuseindoc = '')) AS JobLots ON dbo.jomast.fjobno = JobLots.fcdoc WHERE        (dbo.jomast.fjobno = @jobNo)";
                        break;
                }


                using (var connection = Connection(location))
                {
                    var jobDictionary = new Dictionary<int, Job>();


                    var list = connection.Query<Job, JobLot, Job>(
                        sQuery,
                        (job, joblot) =>
                        {
                            Job jobEntry;

                            if (!jobDictionary.TryGetValue(job.JobId, out jobEntry))
                            {
                                jobEntry = job;
                                jobEntry.Lots = new List<JobLot>();
                                jobDictionary.Add(jobEntry.JobId, jobEntry);
                            }
                            jobEntry.Lots.Add(joblot);


                            return jobEntry;
                        },
                        param: new { jobNo = newJobNo, suffixNo = newSuffix },
                        splitOn: "JobLotId")
                    .Distinct()
                    .ToList();

                    return jobDictionary.FirstOrDefault().Value;

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

