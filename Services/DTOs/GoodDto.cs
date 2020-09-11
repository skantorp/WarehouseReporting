using System;
using System.Collections.Generic;
using System.Text;

namespace Services.DTOs
{
    public class GoodDto
    {
        public int Id { get; set; }
        public double BasePrice { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public int BarCodeNumber { get; set; }
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
    }
}
