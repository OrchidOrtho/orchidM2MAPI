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
    public class EmployeeTitleDataProvider : IEmployeeTitleDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public EmployeeTitleDataProvider(IConfigurationRoot config, ILoggerFactory logger)
        {
            _config = config;
            _logger = logger;
        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:Oracle";
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);

        }

        public async Task<List<EmployeeTitle>> GetAllTitles(string location)
        {
            try
            {
                string sQuery = "";

                sQuery = "SELECT DISTINCT                          COALESCE (OE.[Business title], 'MISSING') AS BusinessTitle,                          CASE OE.[Location] WHEN 'Arab' THEN '072' WHEN 'Arcadia' THEN '052' WHEN 'Bridgeport' THEN '050' WHEN 'Chelsea' THEN '060' WHEN 'Holt' THEN '010' WHEN 'Mason' THEN '080' WHEN 'Oregon' THEN '070' WHEN 'Santa Ana'                           THEN '054' WHEN 'Farmington' THEN '032' WHEN 'Shelton' THEN '082' WHEN 'Memphis' THEN CASE OE.[Business Unit] WHEN 'Orchid Memphis' THEN '030' ELSE '082' END WHEN 'Milford' THEN '074' ELSE '' END AS SiteNo,                           COALESCE (o.DeptName, 'UNKNOWN') AS DeptName FROM            dbo.OracleEE AS OE LEFT OUTER JOIN                          dbo.OOSAEL AS o ON OE.[Employee Number] = o.PersonNumber WHERE        (CASE OE.[Location] WHEN 'Arab' THEN '072' WHEN 'Arcadia' THEN '052' WHEN 'Bridgeport' THEN '050' WHEN 'Chelsea' THEN '060' WHEN 'Holt' THEN '010' WHEN 'Mason' THEN '080' WHEN 'Oregon' THEN '070' WHEN 'Santa Ana'                           THEN '054' WHEN 'Farmington' THEN '032' WHEN 'Shelton' THEN '082' WHEN 'Memphis' THEN CASE OE.[Business Unit] WHEN 'Orchid Memphis' THEN '030' ELSE '082' END WHEN 'Milford' THEN '074' ELSE '' END = @location) GROUP BY COALESCE (OE.[Business title], 'MISSING'),                          CASE OE.[Location] WHEN 'Arab' THEN '072' WHEN 'Arcadia' THEN '052' WHEN 'Bridgeport' THEN '050' WHEN 'Chelsea' THEN '060' WHEN 'Holt' THEN '010' WHEN 'Mason' THEN '080' WHEN 'Oregon' THEN '070' WHEN 'Santa Ana'                           THEN '054' WHEN 'Farmington' THEN '032' WHEN 'Shelton' THEN '082' WHEN 'Memphis' THEN CASE OE.[Business Unit] WHEN 'Orchid Memphis' THEN '030' ELSE '082' END WHEN 'Milford' THEN '074' ELSE '' END,                          o.DeptName";


                using (var connection = Connection(location))
                {
                    var list = connection.Query<EmployeeTitle>(
                        sQuery,
                        param: new { location = location })
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

