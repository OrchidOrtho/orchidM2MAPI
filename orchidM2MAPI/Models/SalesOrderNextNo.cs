using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrderNextNo
    {
        private string _NextSONo;
        private Int32 _NextSOId;
        private string _SalesOrderNo;

        public string SalesOrderNo
        {
            get
            {
                return _SalesOrderNo;
            }
            set
            {
                _NextSOId = Int32.Parse(value) + 1;
                _NextSONo = padNumber6(_NextSOId, true);
                _SalesOrderNo = value;
            }
        }

        public string NextSalesOrderNo
        {
            get
            {
                return _NextSONo;
            }
        }

        private string padNumber6(int numberToPad, bool useZeros)
        {
            string retval = "";

            switch (numberToPad.ToString().Length)
            {
                case 1:
                    if (useZeros)
                        retval = "00000" + numberToPad.ToString();
                    else
                        retval = "     " + numberToPad.ToString();
                    break;
                case 2:
                    if (useZeros)
                        retval = "0000" + numberToPad.ToString();
                    else
                        retval = "    " + numberToPad.ToString();
                    break;
                case 3:
                    if (useZeros)
                        retval = "000" + numberToPad.ToString();
                    else
                        retval = "   " + numberToPad.ToString();
                    break;
                case 4:
                    if (useZeros)
                        retval = "00" + numberToPad.ToString();
                    else
                        retval = "  " + numberToPad.ToString();
                    break;
                case 5:
                    if (useZeros)
                        retval = "0" + numberToPad.ToString();
                    else
                        retval = " " + numberToPad.ToString();
                    break;
                case 6:
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
