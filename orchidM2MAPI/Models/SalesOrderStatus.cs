using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrderStatus
    {
        public SalesOrderStatus()
        {

        }

        private readonly string _currencyId;
        private readonly float _conversionFactor;
        private readonly DateTime _dateOfAlternateCurr;
        private string _NextExternalLineNo;
        private string _NextInternalLineNo;
        private int _NextLineNo;

        [Key]
        public Int32 SalesOrderId { get; set; }

        public string SalesOrderNo { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPONo { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }


    }
}
