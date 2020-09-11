using System;
using System.Collections.Generic;
using System.Text;

namespace Services.DTOs
{
    public class CurrencyDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double ExchangeRate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
