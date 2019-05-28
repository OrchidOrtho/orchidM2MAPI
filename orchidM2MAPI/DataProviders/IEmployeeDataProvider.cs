using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IEmployeeDataProvider
    {
        Task<List<Employee>> GetAllEmployees(string locationo);

        Task<Employee> GetEmployee(string employeeno);

        Task<List<Employee>> UpsertEmployees(Employee[] emps);

    }
}
