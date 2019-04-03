﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class SalesOrderReleases
    {
        [Key]
        public Int32 SalesOrderReleaseId { get; set; }

        public string InternalItemNo { get; set; }
        public string ExternalItemNo { get; set; }
        public string PartNo { get; set; }
        public string PartRev { get; set; }
        public string ReleaseNo { get; set; }
        public string ShipToAddressKey { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsMasterRelease { get; set; }
        public float NetPrice { get; set; }
        public float Quantity { get; set; }
        public bool CanShipBeforeDue { get; set; }
        public bool AllowSplitShipments { get; set; }
        public float UnitPrice { get; set; }
        public bool IsTaxable { get; set; }
        public string DeliveryNotes { get; set; }
        public string UserDefinedRev { get; set; }
        public int Priority { get; set; }

    }
}
