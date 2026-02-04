using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Models
{
    public class MenuItem
    {

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }    

        public int ItemId {  get; set; }

        public MenuItem(string name, decimal price, string category)
        {
            Name = name;
            Price = price;
            Category = category;
        }

        public MenuItem() { }

    }
}
