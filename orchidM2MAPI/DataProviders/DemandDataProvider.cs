using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using orchidM2MAPI.Models;
using Dapper;

namespace orchidM2MAPI.DataProviders
{
    public class DemandDataProvider : IDemandDataProvider
    {
        private readonly IConfigurationRoot _config;

        public DemandDataProvider(IConfigurationRoot config)
        {
            _config = config;
        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:EDW";
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);
        }


        public async Task<List<Demand>> GetDemand(string location, string partNos)
        {
            using (IDbConnection conn = Connection(location))
            {
                string newInClause = "('" + partNos.Replace(",", "','") + "')";

                string sQuery = "SELECT SUM(EDW.FactInvoice.ShipQty) AS TotalQty, EDW.DimOrchidYearPeriod.OrchidYear, EDW.DimOrchidYearPeriod.OrchidPeriod, CASE WHEN LEFT(REVERSE(EDW.DimItem.PartNo), 6) = 'IDNAS-' THEN LEFT(EDW.DimItem.PartNo, LEN(EDW.DimItem.PartNo) - 6) ELSE EDW.DimItem.PartNo END AS ItemNo, EDW.DimItem.PartRev AS ItemRev, EDW.DimOrchidSite.OrchidSiteNumber FROM EDW.DimItem INNER JOIN EDW.DimDate INNER JOIN EDW.DimOrchidYearPeriod ON EDW.DimDate.OrchidYearPeriodId = EDW.DimOrchidYearPeriod.OrchidYearPeriodId INNER JOIN EDW.FactInvoice ON EDW.DimDate.DateId = EDW.FactInvoice.DateId ON EDW.DimItem.ItemId = EDW.FactInvoice.ItemId INNER JOIN EDW.DimVersion ON EDW.FactInvoice.VersionId = EDW.DimVersion.VersionId INNER JOIN EDW.DimOrchidSite ON EDW.DimItem.OrchidSiteId = EDW.DimOrchidSite.OrchidSiteId WHERE (EDW.DimVersion.VersionName = N'Current Demand Forecast') AND (CASE WHEN LEFT(REVERSE(EDW.DimItem.PartNo), 6) = 'IDNAS-' THEN LEFT(EDW.DimItem.PartNo, LEN(EDW.DimItem.PartNo) - 6) ELSE EDW.DimItem.PartNo END IN " + newInClause + ") AND (EDW.DimOrchidSite.OrchidSiteNumber = @siteno) AND (EDW.DimOrchidYearPeriod.OrchidYear >= YEAR(GETDATE())) GROUP BY EDW.DimOrchidYearPeriod.OrchidYear, EDW.DimOrchidYearPeriod.OrchidPeriod, EDW.DimItem.PartRev, EDW.DimOrchidSite.OrchidSiteNumber, CASE WHEN LEFT(REVERSE(EDW.DimItem.PartNo), 6) = 'IDNAS-' THEN LEFT(EDW.DimItem.PartNo, LEN(EDW.DimItem.PartNo) - 6) ELSE EDW.DimItem.PartNo END";
                conn.Open();
                var result = await conn.QueryAsync<Demand>(sQuery, new { siteno = location });

                return result.ToList();
                //QueryFirstOrDefaultAsync
            }
        }

    }
}

