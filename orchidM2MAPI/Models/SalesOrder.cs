using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrder
    {
        public SalesOrder()
        {
            _currencyId = "USD";
            _conversionFactor = 1;
            _dateOfAlternateCurr = DateTime.Parse("1/1/1900");
            Priority = 4;
            Status = "Started";
            CreatedDate = DateTime.Today;
            ModifiedDate = DateTime.Today;
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

        public string SoldToCity { get; set; }
        public string SoldToCountry { get; set; }
        public string SoldToState { get; set; }
        public string SoldToZip { get; set; }
        public string SoldToStreet { get; set; }
        public string ContactName { get; set; }
        public string UserDefinedString1 { get; set; }
        public string UserDefinedstring2 { get; set; }
        public string UserDefinedString3 { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? UserDefinedDate1 { get; set; }
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

        public List<SalesOrderItem> SalesOrderLineItems { get; set; }


        public int NextLineNo
        {
            get
            {
                return _NextLineNo;
            }
            set
            {
                _NextLineNo = value;
                _NextExternalLineNo = padNumber3(value, true);
                _NextInternalLineNo = padNumber3(value, false);
            }
        }

        public string NextExternalLineNo
        {
            get
            {
                return _NextExternalLineNo;
            }
        }

        public string NextInternalLineNo
        {
            get
            {
                return _NextInternalLineNo;
            }
        }

        public bool IsNew
        {
            get
            {
                if (SalesOrderId == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private string padNumber3(int numberToPad, bool useZeros)
        {
            string retval = "";

            switch (numberToPad.ToString().Length)
            {
                case 1:
                    if (useZeros)
                        retval = "00" + numberToPad.ToString();
                    else
                        retval = "  " + numberToPad.ToString();
                    break;
                case 2:
                    if (useZeros)
                        retval = "0" + numberToPad.ToString();
                    else
                        retval = " " + numberToPad.ToString();
                    break;
                case 3:
                    retval = numberToPad.ToString();
                    break;
                default:
                    retval = "";
                    break;
            }
            return retval;
        }

    }
}
