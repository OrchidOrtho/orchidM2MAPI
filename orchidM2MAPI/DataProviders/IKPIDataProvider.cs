using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IKPIDataProvider
    {
        Task<float> GetKPIValue(KPI kpiItem);


    }
}
