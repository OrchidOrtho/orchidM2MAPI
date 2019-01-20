using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class ReceivingItem
    {
        public Int32 ReceivingItemId { get; set; }
        public string ReceivingItemType { get; set; }
        public string ReceivingItemLineNo { get; set; }
        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string Facility { get; set; }
        public float TotalQuantityReceived { get; set; }
        public string InventoryLocation { get; set; }
        public string QuantityUnitofMeasure { get; set; }
        public string Comments { get; set; }
        public string POItemNumber { get; set; }
        public string InspectFlag { get; set; }

        public List<ReceivingItemLot> Lots { get; set; }
    }
}
