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
    public class KPIDataProvider : IKPIDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public KPIDataProvider(IConfigurationRoot config, ILoggerFactory logger)
        {
            _config = config;
            _logger = logger;
        }

        public IDbConnection Connection(string connectionstring)
        {
            return new SqlConnection(connectionstring);
        }

        public async Task<float> GetKPIValue(KPI kpiItem)
        {
            try
            {
                string sQuery = "";

                sQuery = kpiItem.SQLStatement;


                using (var connection = Connection(kpiItem.ConnectionString))
                {
                    var retVal = connection.QueryFirst<float>(
                        sQuery);

                    return retVal;

                }

            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return float.NaN;
            }

        }

    }
}

