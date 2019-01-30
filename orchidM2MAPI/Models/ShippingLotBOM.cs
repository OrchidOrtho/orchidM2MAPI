using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ShippingLotBOM
    {

        public Int32 BOMLevel { get; set; }
        public string BOMLotNo { get; set; }
        public string BOMPartNo { get; set; }
        public string BOMPartRev { get; set; }
        public string BOMPartDesc { get; set; }
        public string Purchased { get; set; }
        public float BOMQuantity { get; set; }
        public string BOMUnitofMeasure { get; set; }
        public string PartGroup { get; set; }
    }
}
