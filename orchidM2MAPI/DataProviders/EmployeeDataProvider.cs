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
    public class EmployeeDataProvider : IEmployeeDataProvider
    {
        private readonly IConfigurationRoot _config;
        private readonly ILoggerFactory _logger;

        public EmployeeDataProvider(IConfigurationRoot config, ILoggerFactory logger)
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

        public async Task<Employee> GetEmployee(string employeeno)
        {
            try
            {
                string sQuery = "";
                sQuery = "SELECT DISTINCT                          OE.OracleEE_ID AS EmployeeId, OE.[Employee first name] AS FirstName, OE.[Employee Last Name] AS LastName, OE.[Employee middle name initial] AS MiddleInitial, OE.[Employee e-mail] AS Email, OE.[Job title] AS Title,                          OE.Shift, RIGHT(OE.[Assignment Number], LEN(OE.[Assignment Number]) - 1) AS EmployeeNo, CASE OE.[Employee type] WHEN 'Hourly' THEN 'H' ELSE 'S' END AS EmployeeType, COALESCE                              ((SELECT        TOP (1) [Employee first name] + ' ' + [Employee Last Name] AS Expr1                                  FROM            dbo.OracleEE AS OS                                  WHERE        (OE.Supervisor = [Employee Number])), 'MISSING') AS Supervisor, CASE OE.[Employment type] WHEN 'ACTIVE' THEN 'A' ELSE 'I' END AS Status, COALESCE (OE.[Business title], 'MISSING') AS BusinessTitle,                          COALESCE (o.DeptName, 'UNKNOWN') AS DeptName,                          CASE OE.[Location] WHEN 'Arab' THEN '072' WHEN 'Arcadia' THEN '052' WHEN 'Bridgeport' THEN '050' WHEN 'Chelsea' THEN '060' WHEN 'Holt' THEN '010' WHEN 'Mason' THEN '080' WHEN 'Oregon' THEN '070' WHEN 'Santa Ana'                           THEN '054' WHEN 'Farmington' THEN '032' WHEN 'Shelton' THEN '082' WHEN 'Memphis' THEN CASE OE.[Business Unit] WHEN 'Orchid Memphis' THEN '030' ELSE '082' END WHEN 'Milford' THEN '074' ELSE '' END AS SiteNo FROM            dbo.OracleEE AS OE LEFT OUTER JOIN                          dbo.OOSAEL AS o ON OE.[Employee Number] = o.PersonNumber WHERE        (RIGHT(OE.[Assignment Number], LEN(OE.[Assignment Number]) - 1) = @employeeno)";



                using (var connection = Connection("Oracle"))
                {


                    var emp = connection.Query<Employee>(
                        sQuery,

                        param: new { employeeno = employeeno })
                    .Distinct();

                    return emp.FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _logger.CreateLogger("error").Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

        public async Task<List<Employee>> GetAllEmployees(string location)
        {
            try
            {
                string sQuery = "";

                sQuery = "SELECT DISTINCT                          OE.OracleEE_ID AS EmployeeId, OE.[Employee first name] AS FirstName, OE.[Employee Last Name] AS LastName, OE.[Employee middle name initial] AS MiddleInitial, OE.[Employee e-mail] AS Email, OE.[Job title] AS Title,                          OE.Shift, RIGHT(OE.[Assignment Number], LEN(OE.[Assignment Number]) - 1) AS EmployeeNo, CASE OE.[Employee type] WHEN 'Hourly' THEN 'H' ELSE 'S' END AS EmployeeType, COALESCE                              ((SELECT        TOP (1) [Employee first name] + ' ' + [Employee Last Name] AS Expr1                                  FROM            dbo.OracleEE AS OS                                  WHERE        (OE.Supervisor = [Employee Number])), 'MISSING') AS Supervisor, CASE OE.[Employment type] WHEN 'ACTIVE' THEN 'A' ELSE 'I' END AS Status, COALESCE (OE.[Business title], 'MISSING') AS BusinessTitle,                          COALESCE (o.DeptName, N'UNKNOWN') AS DeptName,                          CASE OE.[Location] WHEN 'Arab' THEN '072' WHEN 'Arcadia' THEN '052' WHEN 'Bridgeport' THEN '050' WHEN 'Chelsea' THEN '060' WHEN 'Holt' THEN '010' WHEN 'Mason' THEN '080' WHEN 'Oregon' THEN '070' WHEN 'Santa Ana'                           THEN '054' WHEN 'Farmington' THEN '032' WHEN 'Shelton' THEN '082' WHEN 'Memphis' THEN CASE OE.[Business Unit] WHEN 'Orchid Memphis' THEN '030' ELSE '082' END WHEN 'Milford' THEN '074' ELSE '' END AS SiteNo FROM            dbo.OracleEE AS OE LEFT OUTER JOIN                          dbo.OOSAEL AS o ON OE.[Employee Number] = o.PersonNumber WHERE        (CASE OE.[Location] WHEN 'Arab' THEN '072' WHEN 'Arcadia' THEN '052' WHEN 'Bridgeport' THEN '050' WHEN 'Chelsea' THEN '060' WHEN 'Holt' THEN '010' WHEN 'Mason' THEN '080' WHEN 'Oregon' THEN '070' WHEN 'Santa Ana'                           THEN '054' WHEN 'Farmington' THEN '032' WHEN 'Shelton' THEN '082' WHEN 'Memphis' THEN CASE OE.[Business Unit] WHEN 'Orchid Memphis' THEN '030' ELSE '082' END WHEN 'Milford' THEN '074' ELSE '' END = @location)";

                using (var connection = Connection("Oracle"))
                {
                    var list = connection.Query<Employee>(
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

        public async Task<List<Employee>> UpsertEmployees(Employee[] employeeList)
        {

            using (var connection = Connection("Oracle"))
            {
                foreach (Employee emp in employeeList)
                {
                    //Insert or Update the Employee using a stored procedure
                    connection.Execute("WorkdayUpsert", new { @workerId = emp.EmployeeId, @firstName = emp.FirstName, @lastName = emp.LastName }, commandType: CommandType.StoredProcedure);

                }

            }



            return null;
        }
    }

}

