using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IOrderInventoryCoordination
    {

        OrderLineItem? DecrementOnCreation(MenuItem item, Order order, int quantity);

        void IncrementOnDeletion(OrderLineItem item);

        void DecrementOnQuantityChanged(OrderLineItem item, int quantity);

    }
}
