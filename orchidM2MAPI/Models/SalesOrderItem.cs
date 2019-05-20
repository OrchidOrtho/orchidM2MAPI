using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrderItem
    {
        private string _internalItemNo;
        private string _externalItemNo;

        [Key]
        public Int32 SalesOrderItemId { get; set; }

        public string SalesOrderItemNo { get; set; }
        public string InternalItemNo
        {
            get
            {
                return _internalItemNo;
            }
            set
            {
                string temp = value;
                if (temp.Length < 3)
                    _internalItemNo = padNumber3(temp, false);
                else
                    _internalItemNo = value;
            }
        }

        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string CustomerPartNo { get; set; }
        public string CustomerPartRev { get; set; }
        public DateTime DueDateToShip { get; set; }
        public float Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public string PartDesc { get; set; }
        public string PartGroup { get; set; }

        public string TextLoc { get; set; }
        public bool LotRequired { get; set; }
        public string ExternalItemNo
        {
            get
            {
                return _externalItemNo;
            }
            set
            {
                string temp = value;
                if (temp.Length < 3)
                    _externalItemNo = padNumber3(temp, true);
                else
                    _externalItemNo = value;
            }
        }
        public bool IsBlanketRelease { get; set; }
        public float QuantityOver { get; set; }
        public float QuantityUnder { get; set; }
        public bool PrintMemo { get; set; }
        public string PartClass { get; set; }
        public string PartSource { get; set; }
        public string DescriptionShort { get; set; }
        public string DescriptionMemo { get; set; }
        public string Facility { get; set; }
        public string SourceFacility { get; set; }
        public string UserDefinedRev { get; set; }
        public float AlternateQuantity { get; set; }
        public string AlternateUnitOfMeasure { get; set; }

        public List<SalesOrderReleases> SalesOrderReleases { get; set; }


        private string padNumber3(string numberToPad, bool useZeros)
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
