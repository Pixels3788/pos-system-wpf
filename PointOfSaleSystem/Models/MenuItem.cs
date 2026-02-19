using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text;


// Data class that models the menu items from the database and makes them easy to manage and present
namespace PointOfSaleSystem.Models
{
    public class MenuItem : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }    

        public int ItemId {  get; set; }

        private bool _isAvailable;

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        public MenuItem(string name, decimal price, string category)
        {
            Name = name;
            Price = price;
            Category = category;
        }

        public MenuItem() { }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}
