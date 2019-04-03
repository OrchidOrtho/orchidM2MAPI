using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrderItem
    {
        [Key]
        public Int32 SalesOrderItemId { get; set; }

        public string SalesOrderItemNo { get; set; }
        public string InternalItemNo { get; set; }
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
        public string ExternalItemNo { get; set; }
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

    }
}
