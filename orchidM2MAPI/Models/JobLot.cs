using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace orchidM2MAPI.Models
{
    public class JobLot
    {
        [Key]
        public Int32 JobLotId { get; set; }
        public string LotNo { get; set; }
        public float LotQuantity { get; set; }
        public string LotQtyUnitOfMeasure { get; set; }

    }
}
