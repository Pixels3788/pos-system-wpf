using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IOrderService
    {
        OrderLineItem? CreateOrderLineItem(MenuItem menuItem, Order order, int quantity);

        Order CreateNewOrder();

        void DeleteOrder(int orderId);

        void DeleteLineItem(int lineItemId);

        OrderLineItem? UpdateOrderLineItemQuantity(int lineItemId, int quantity);

        List<Order> LoadOrders(bool includeLineItems = true);

        List<OrderLineItem> LoadOrderItems();

        Order? FinalizeOrder(int orderId);

        Order? GetOrderById(int orderId);

        List<OrderLineItem> GetOrderLineItemsByOrder(int orderId);

        OrderLineItem? GetOrderLineItemById(int lineItemId);

        List<Order> GetOpenOrders(bool includeLineItems = true);

        List<Order> GetFinalizedOrders(bool includeLineItems = true);
    }
}
