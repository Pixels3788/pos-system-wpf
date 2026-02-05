using Dapper;
using Microsoft.Data.Sqlite;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Models;
using System.Collections.ObjectModel;
using System;

using System.Text;
using PointOfSaleSystem.Services.Interfaces;

namespace PointOfSaleSystem.Services
{
    public class OrderService : IOrderService
    {

        private DbManager _dbManager;

        public OrderService(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public OrderLineItem? CreateOrderLineItem(MenuItem menuItem, Order order, int quantity)
        {

            if (menuItem == null) return null;

            if (order == null) return null;

            if (quantity < 1) return null;

            int newOrderId = order.OrderId;
            OrderLineItem newItem = new OrderLineItem(menuItem, quantity, newOrderId);

            using var connection = _dbManager.GetConnection();

            var sql = "INSERT INTO OrderLineItems (OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity) VALUES (@OrderId, @MenuItemId, @NameAtSale, @UnitPrice, @Quantity); " +
                      "SELECT last_insert_rowid();";

            newItem.LineItemId = connection.ExecuteScalar<int>(sql, newItem);
            return newItem;
        }

        public Order CreateNewOrder()
        {
            Order newOrder = new Order();

            using var connection = _dbManager.GetConnection();

            var sql = "INSERT INTO Orders (CreatedAt) VALUES (@CreatedAt); " +
                      "SELECT last_insert_rowid();";

            newOrder.OrderId = connection.ExecuteScalar<int>(sql, newOrder);
            return newOrder;
        }

        public void DeleteOrder(int orderId) 
        {
            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "DELETE FROM Orders WHERE OrderId = @OrderId", new {OrderId = orderId}
            );
        }

        public void DeleteLineItem(int  lineItemId)
        {
            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "DELETE FROM OrderLineItems WHERE LineItemId = @LineItemId", new {LineItemId = lineItemId}
            );
        }

        public OrderLineItem? UpdateOrderLineItemQuantity(int lineItemId, int quantity)
        {

            if (quantity < 1) return null;

            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "UPDATE OrderLineItems SET Quantity = @Quantity WHERE LineItemId = @LineItemId",
                new {LineItemId = lineItemId, Quantity = quantity}
            );

            var updatedItem = GetOrderLineItemById(lineItemId);

            return updatedItem;
        }

        public List<Order> LoadOrders(bool includeLineItems = true)
        {
            using var connection = _dbManager.GetConnection();

            string ordersQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders";

            var orders = connection.Query<Order>(ordersQuery).ToList();

            if (includeLineItems)
            {
                foreach (var order in orders)
                {
                    var lineItems = GetOrderLineItemsByOrder(order.OrderId);
                    order.LineOrder = new ObservableCollection<OrderLineItem>(lineItems);
                }
            }

            return orders;
        }

        public List<OrderLineItem> LoadOrderItems()
        {
            using var connection = _dbManager.GetConnection();

            string orderItemsQuery = "SELECT LineItemId, OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity FROM OrderLineItems";

            var orderItems = connection.Query<OrderLineItem>(orderItemsQuery).ToList();

            return orderItems;
        }

        public Order? FinalizeOrder(int orderId)
        {
            using var connection = _dbManager.GetConnection();

            connection.Execute(
               "UPDATE Orders SET IsFinalized = 1, FinalizedAt = @FinalizedAt WHERE OrderId = @OrderId",
                new { FinalizedAt = DateTime.Now, OrderId = orderId }
            );

            return GetOrderById(orderId);
        }

        public Order? GetOrderById(int orderId) 
        {
            using var connection = _dbManager.GetConnection();

            string getOrderQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders WHERE OrderId = @OrderId";

            var retrievedOrder = connection.QueryFirstOrDefault<Order>(getOrderQuery, new {OrderId = orderId});

            if (retrievedOrder == null) return null;

            List<OrderLineItem> retrievedOrderItems = GetOrderLineItemsByOrder(retrievedOrder.OrderId);

            retrievedOrder.LineOrder = new ObservableCollection<OrderLineItem>(retrievedOrderItems);

            return retrievedOrder;
        }

        public List<OrderLineItem> GetOrderLineItemsByOrder(int orderId)
        {
            using var connection = _dbManager.GetConnection();

            string getLineOrderItemsQuery = "SELECT LineItemId, OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity FROM OrderLineItems WHERE OrderId = @OrderId";

            var retrievedItems = connection.Query<OrderLineItem>(getLineOrderItemsQuery, new {OrderId = orderId}).ToList();

            return retrievedItems;
        }

        public OrderLineItem? GetOrderLineItemById(int lineItemId) 
        {
            using var connection = _dbManager.GetConnection();

            string getOrderLineItemQuery = "SELECT LineItemId, OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity FROM OrderLineItems WHERE LineItemId = @LineItemId";

            var retrievedItem = connection.QueryFirstOrDefault<OrderLineItem>(getOrderLineItemQuery, new { LineItemId = lineItemId });

            return retrievedItem;
        }

        public List<Order> GetOpenOrders(bool includeLineItems = true)
        {
            using var connection = _dbManager.GetConnection();

            string getOpenOrdersQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders WHERE IsFinalized = 0";

            var openOrders = connection.Query<Order>(getOpenOrdersQuery).ToList();

            if (includeLineItems)
            {
                foreach (var order in openOrders)
                {
                    var lineItems = GetOrderLineItemsByOrder(order.OrderId);
                    order.LineOrder = new ObservableCollection<OrderLineItem>(lineItems);
                }
            }

            return openOrders;
        }

        public List<Order> GetFinalizedOrders(bool includeLineItems = true) 
        {
            using var connection = _dbManager.GetConnection();

            string getFinalizedOrdersQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders WHERE IsFinalized = 1";

            var finalizedOrders = connection.Query<Order>(getFinalizedOrdersQuery).ToList();

            if (includeLineItems)
            {
                foreach (var order in finalizedOrders)
                {
                    var lineItems = GetOrderLineItemsByOrder(order.OrderId);
                    order.LineOrder = new ObservableCollection<OrderLineItem>(lineItems);
                }
            }

            return finalizedOrders;
        }

    }
}
