using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderLineItem?> CreateOrderLineItem(MenuItem menuItem, Order order, int quantity);

        Task<Order?> CreateNewOrder();

        Task DeleteOrder(int orderId);

        Task DeleteLineItem(int lineItemId);

        Task<OrderLineItem?> UpdateOrderLineItemQuantity(int lineItemId, int quantity);

        Task<List<Order>> LoadOrders(bool includeLineItems = true);

        Task<List<OrderLineItem>> LoadOrderItems();

        Task<Order?> FinalizeOrder(int orderId);

        Task<Order?> GetOrderById(int orderId);

        Task<List<OrderLineItem>> GetOrderLineItemsByOrder(int orderId);

        Task<OrderLineItem?> GetOrderLineItemById(int lineItemId);

        Task<List<Order>> GetOpenOrders(bool includeLineItems = true);

        Task<List<Order>> GetFinalizedOrders(bool includeLineItems = true);
    }
}
