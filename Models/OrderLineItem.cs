using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace PointOfSaleSystem.Models
{
    public class OrderLineItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int MenuItemId { get; set; }

        public string NameAtSale { get; set; }

        public int LineItemId { get; set; }

        public int OrderId { get; set; }

        private decimal _unitPrice;
        public decimal UnitPrice 
        { 
            get { return _unitPrice; }
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        private int _quantity;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }
        
        public decimal LineTotal { get => UnitPrice * Quantity;  }

        public OrderLineItem(MenuItem menuItem, int quantity, int orderId)
        {
            MenuItemId = menuItem.ItemId;
            NameAtSale = menuItem.Name;
            UnitPrice = menuItem.Price;
            OrderId = orderId;
            Quantity = quantity;
        }

        public OrderLineItem() { }

        public void increment(int amount)
        {
            Quantity += amount;
        }
        public void decrement(int amount)
        {
            Quantity = Math.Max(0, Quantity - amount);
        }

        protected void OnPropertyChanged(string propertyName) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
