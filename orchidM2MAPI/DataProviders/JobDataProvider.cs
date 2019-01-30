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
                using (IDbConnection conn = Connection(location))
                {
                    string sQuery = "SELECT dbo.jomast.fjobno AS JobNo, RTRIM(dbo.jomast.fpartno) AS PartNo, RTRIM(dbo.jomast.fpartrev) AS PartRev, RTRIM(dbo.jomast.fac) AS Facility, RTRIM(dbo.inmast.fgroup) AS PartFamily FROM dbo.jomast INNER JOIN dbo.inmast ON dbo.jomast.fpartno = dbo.inmast.fpartno AND dbo.jomast.fpartrev = dbo.inmast.frev AND dbo.jomast.fac = dbo.inmast.fac WHERE dbo.jomast.fjobno = @jobNo ";
                    conn.Open();
                    var result = await conn.QueryAsync<Job>(sQuery, new { jobNo = JobNo });

                    return result.FirstOrDefault();
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

