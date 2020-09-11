using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BusinessObjects.Entities
{
    public class Good
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public int BarCodeNumber { get; set; }
        [ForeignKey("Currency")]
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public CategoryOfGood Category { get; set; }
        public virtual ICollection<GoodWarehouse> GoodWarehouses { get; set; } = new List<GoodWarehouse>();
    }
}
