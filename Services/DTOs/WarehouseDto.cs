using System;
using System.Collections.Generic;
using System.Text;

namespace Services.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public WarehouseGoodDto[] Goods { get; set; } = new WarehouseGoodDto[0];
    }
    public class WarehouseGoodDto
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int GoodId { get; set; }
    }
}
