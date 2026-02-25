using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proceso_etl.Models.Csv
{
    public class CustomerCsvModel
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public CustomerCsvModel()
        {
            
        }
        public CustomerCsvModel(int customerId, string firstName, string lastName, string email,
                     string phone, string city, string country)
        {
            CustomerID = customerId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            City = city;
            Country = country;
        }

    }
}
