using System.Collections.Generic;
using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface IGaugeDataProvider
    {
        Task<List<GaugeType>> GetGaugeTypes(string location);

        Task<List<Gauge>> GetGauges(string location);
    }
}
