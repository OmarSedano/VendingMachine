

using System;

namespace VendingMachineApp.Domain.Entities
{
    public class Product
    {
        public Product(int id,string name, decimal price, int quantity)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public int Id { get;  }

        public string Name { get;  }

        public decimal Price { get;  }

        public int Quantity { get; set; }
    }
}
