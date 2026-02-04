using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace PointOfSaleSystem.Models
{
    public class InventoryItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public MenuItem? MenuItem { get; set; }

        private int _quantityOnHand;

        public int InventoryItemId { get;  set; }

        public int MenuItemId { get; set; }

        public int QuantityOnHand
        {
            get { return _quantityOnHand; }
            private set
            {
                if (_quantityOnHand != value)
                {
                    _quantityOnHand = value;
                    OnPropertyChanged(nameof(QuantityOnHand));
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }

        public bool IsAvailable => QuantityOnHand > 0;

        public InventoryItem(MenuItem item, int quantityOnHand)
        {
            MenuItem = item;
            MenuItemId = item.ItemId;
            QuantityOnHand = quantityOnHand;
        }

        public InventoryItem(int inventoryItemId, int quantityOnHand, int menuItemId)
        {
            InventoryItemId = inventoryItemId;
            QuantityOnHand = quantityOnHand;
            MenuItemId = menuItemId;
        }

        public InventoryItem() { }

        public void Increment(int amount)
        {
            QuantityOnHand += amount;
            
        }

        public void Decrement(int amount)
        {
            QuantityOnHand = Math.Max(0, QuantityOnHand - amount);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
