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

                // Retrieve shipping info
                switch (location)
                {
                    case "072":
                        //Syteline Query
                        sQuery = "SELECT        TOP (100) PERCENT dbo.job.job, dbo.job.suffix, dbo.jobroute.oper_num, dbo.job.item, dbo.jobroute.wc, dbo.wc.description FROM            dbo.jobroute INNER JOIN                          dbo.job ON dbo.jobroute.job = dbo.job.job AND dbo.jobroute.suffix = dbo.job.suffix INNER JOIN                          dbo.wc ON dbo.jobroute.wc = dbo.wc.wc WHERE        (dbo.job.job = @jobNo) AND (dbo.job.suffix = 0001) ORDER BY dbo.job.suffix, dbo.job.job_date DESC, dbo.jobroute.oper_num";
                        break;
                    default:
                        sQuery = "SELECT        dbo.jomast.identity_column AS JobId, dbo.jomast.fjobno AS JobNo, RTRIM(dbo.jomast.fpartno) AS PartNo, RTRIM(dbo.jomast.fpartrev) AS PartRev, dbo.jomast.ftype AS JobType, RTRIM(dbo.jomast.fstatus) AS JobStatus,                          dbo.jomast.fddue_date AS JobDueDate, dbo.jomast.fquantity AS JobQuantity, JobLots.identity_column AS JobLotId, RTRIM(JobLots.fclot) AS LotNo, JobLots.fnquantity AS LotQuantity, RTRIM(JobLots.fcmeasure) AS LotQtyUnitOfMeasure FROM            dbo.jomast LEFT OUTER JOIN                              (SELECT        fcdoc, fcpartno, fcpartrev, fclot, fnquantity, fddate, fcmeasure, identity_column, fac                                FROM            dbo.qalotc                                WHERE        (fctype = 'J') AND (fcuseindoc = '')) AS JobLots ON dbo.jomast.fjobno = JobLots.fcdoc WHERE        (dbo.jomast.fjobno = @jobNo)";
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
                        param: new { jobNo = JobNo },
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

