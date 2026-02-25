using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proceso_etl.Models.Csv
{
    public class OrderDetailsCsvModel
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderDetailsCsvModel()
        {
            
        }
        public OrderDetailsCsvModel(int orderId, int productId, int quantity, decimal totalPrice)
        {
            OrderID = orderId;
            ProductID = productId;
            Quantity = quantity;
            TotalPrice = totalPrice;
        }

    }
}
