using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proceso_etl.Models.Csv
{
    public class ProductCsvModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public ProductCsvModel()
        {
            
        }
        public ProductCsvModel(int productId, string productName, string category, decimal price, int stock)
        {
            ProductID = productId;
            ProductName = productName;
            Category = category;
            Price = price;
            Stock = stock;
        }
    }
}
