using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.RightsManagement;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PointOfSaleSystem.Models
{
    public class Order : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<OrderLineItem> LineOrder { get; set; } = new();

        public decimal OrderTotal { get => LineOrder.Sum(item => item.LineTotal); }

        public decimal TotalAfterTax { get => Math.Round(OrderTotal * 1.08m, 2); }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? FinalizedAt { get; set; }

        public bool IsFinalized { get; set; }

        public int OrderId { get; set; }

        public Order()
        {
            LineOrder.CollectionChanged += LineOrder_CollectionChanged;
        }
        



        private void LineOrder_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OrderLineItem item in e.NewItems)
                {
                    item.PropertyChanged += LineItem_PropertyChanged;   
                }
            }

            if (e.OldItems != null)
            {
                foreach (OrderLineItem item in e.OldItems)
                {
                    item.PropertyChanged -= LineItem_PropertyChanged;
                }
            }

            OnPropertyChanged(nameof(OrderTotal));
        }

        public void FinalizeOrder()
        {
            IsFinalized = true;
            FinalizedAt = DateTime.Now;
            OnPropertyChanged(nameof(IsFinalized));
            OnPropertyChanged(nameof(FinalizedAt));
        }


        private void LineItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(OrderTotal));
        }





        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
