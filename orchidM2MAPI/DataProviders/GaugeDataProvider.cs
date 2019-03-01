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
    public class GaugeDataProvider : IGaugeDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public GaugeDataProvider(IConfigurationRoot config, ILoggerFactory logger)
        {
            _config = config;
            _logger = logger;
        }

        public IDbConnection Connection(string location)
        {
            string configSection = "ConnectionStrings:MQ1";
            string connectionString = _config[configSection];
            return new SqlConnection(connectionString);
        }

        public async Task<List<GaugeType>> GetGaugeTypes(string location)
        {
            try
            {
                string sQuery = "";

                sQuery = "SELECT        dbo.Sites_p.SiteAbbreviation_f AS Site, dbo.EquipmentSubcategories_p.EquipmentSubcategory_f AS GaugeTypeName, dbo.Equipment_p.EquipmentSubcategory_f AS GaugeTypeId, dbo.__State.Name AS Status FROM            dbo.Equipment_p INNER JOIN                          dbo.EquipmentSubcategories_p ON dbo.Equipment_p.EquipmentSubcategory_f = dbo.EquipmentSubcategories_p.Id INNER JOIN                          dbo.Sites_p ON dbo.Equipment_p.Site_f = dbo.Sites_p.Id INNER JOIN                          dbo.__State ON dbo.EquipmentSubcategories_p.CurrentState = dbo.__State.Id WHERE        (dbo.Equipment_p.IsRemoved = 0) AND (dbo.Sites_p.SiteAbbreviation_f = @location) GROUP BY dbo.Sites_p.SiteAbbreviation_f, dbo.EquipmentSubcategories_p.EquipmentSubcategory_f, dbo.Equipment_p.EquipmentSubcategory_f, dbo.__State.Name";


                using (var connection = Connection(location))
                {
                    var list = connection.Query<GaugeType>(
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

        public async Task<List<Gauge>> GetGauges(string location)
        {
            try
            {
                string sQuery = "";


                sQuery = "SELECT        dbo.Equipment_p.EquipmentNo_f AS GaugeNo, dbo.Sites_p.SiteAbbreviation_f AS Site, dbo.EquipmentSubcategories_p.EquipmentSubcategory_f AS GaugeSubcategory,                          dbo.Equipment_p.EquipmentSubcategory_f AS GaugeSubcategoryId, dbo.Equipment_p.EquipmentName_f AS GaugeName, dbo.GageStatus_p.Status_f AS Status, dbo.Equipment_p.Id AS GaugeId FROM            dbo.Equipment_p INNER JOIN                          dbo.EquipmentSubcategories_p ON dbo.Equipment_p.EquipmentSubcategory_f = dbo.EquipmentSubcategories_p.Id INNER JOIN                          dbo.Sites_p ON dbo.Equipment_p.Site_f = dbo.Sites_p.Id LEFT OUTER JOIN                          dbo.GageStatus_p ON dbo.Equipment_p.GagesStatus_f = dbo.GageStatus_p.Id WHERE        (dbo.Equipment_p.IsRemoved = 0) AND (dbo.Sites_p.SiteAbbreviation_f = @location)";


                using (var connection = Connection(location))
                {
                    var list = connection.Query<Gauge>(
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

