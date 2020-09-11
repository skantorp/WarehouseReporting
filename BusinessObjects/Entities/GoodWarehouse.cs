using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BusinessObjects.Entities
{
    public class GoodWarehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Good")]
        public int GoodId { get; set; }

        public Good Good { get; set; }

        [Required]
        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; }
        public int Amount { get; set; }
    }
}
