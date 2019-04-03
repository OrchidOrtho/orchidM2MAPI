using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrder
    {
        [Key]
        public Int32 SalesOrderId { get; set; }

        public string SalesOrderNo { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPONo { get; set; }
        public DateTime AcknowledgedDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string BillingStreet { get; set; }
        public DateTime CancelledDate { get; set; }
        public string ContactName { get; set; }
        public string UserDefinedString1 { get; set; }
        public string UserDefinedstring2 { get; set; }
        public string UserDefinedString3 { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime UserDefinedDate1 { get; set; }
        public string FaxNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string FreightOnBoardPoint { get; set; }
        public float UserDefinedQuantity1 { get; set; }
        public float UserDefinedCurrency1 { get; set; }
        public string PaymentType { get; set; }
        public string ShipVia { get; set; }
        public string ShipToAddressKey { get; set; }
        public string SoldToAddressKey { get; set; }
        public string BillToAddressKey { get; set; }
        public string SalesOrderCoordinator { get; set; }
        public string SoldBy { get; set; }
        public string Terms { get; set; }
        public string InternalSalesTerritory { get; set; }
        public string CustomerSONumber { get; set; }
        public string SOAcknowledgeMemo { get; set; }
        public string UserDefinedMemo1 { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<SalesOrderItem> Items { get; set; }
        public List<SalesOrderReleases> Releases { get; set; }
    }
}
