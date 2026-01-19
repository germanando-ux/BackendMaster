using System;
using System.Collections.Generic;
using System.Text;

namespace Store.Data.DTO
{
    public class StockReportDto
    {
        public string CategoryName { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
    }
}
