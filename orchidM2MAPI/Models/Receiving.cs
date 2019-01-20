using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace orchidM2MAPI.Models
{
    public class Receiving
    {

        public string ReceiverNo { get; set; }

        [Description("The unique identifier of the vendor.")]
        public string VendorNo { get; set; }

        [Key]
        public Int32 ReceivingId { get; set; }
        public string PONumber { get; set; }
        public DateTime DateReceived { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }

        public List<ReceivingItem> ReceivingItems { get; set; }
    }
}
