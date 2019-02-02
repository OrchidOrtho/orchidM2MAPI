using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IEmployeeTitleDataProvider
    {
        Task<List<EmployeeTitle>> GetAllTitles(string location);


    }
}
