using System.Threading.Tasks;
using orchidM2MAPI.Models;

namespace orchidM2MAPI.DataProviders
{
    public interface ISalesOrderDataProvider
    {
        Task<SalesOrder> GetSalesOrder(string location, string CustomerPONo, string CustomerNo);

        Task<SalesOrderItem> UpdateSalesOrderDetail(string location, SalesOrderItem item);

    }
}
