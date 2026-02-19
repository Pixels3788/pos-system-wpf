using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

// public interface for the order inventory coordinator
namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IOrderInventoryCoordination
    {

        Task<OrderLineItem?> DecrementOnCreation(MenuItem item, Order order, int quantity);

        Task IncrementOnDeletion(OrderLineItem item);

        Task DecrementOnQuantityChanged(OrderLineItem item, int quantity);

    }
}
