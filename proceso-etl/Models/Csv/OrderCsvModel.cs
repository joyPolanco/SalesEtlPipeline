using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proceso_etl.Models.Csv
{
    public class OrderCsvModel
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public string OrderDate { get; set; }
        public string Status { get; set; }
        public OrderCsvModel()
        {
            
        }
        public OrderCsvModel(int orderId, int customerId, string orderDate, string status)
        {
            OrderID = orderId;
            CustomerID = customerId;
            OrderDate = orderDate;
            Status = status;
        }
    }
}
