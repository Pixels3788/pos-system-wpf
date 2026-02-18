using Dapper;
using Microsoft.Data.Sqlite;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Models;
using System.Collections.ObjectModel;
using System;
using Serilog;
using System.Text;
using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Database.Interfaces;

namespace PointOfSaleSystem.Services
{
    public class OrderService : IOrderService
    {

        private IDbManager _dbManager;

        public OrderService(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<OrderLineItem?> CreateOrderLineItem(MenuItem menuItem, Order order, int quantity)
        {

            if (menuItem == null)
            {
                Log.Warning("Order Line Item Creation Failed: Invalid menu item used for creation");
                return null;
            }

            if (order == null)
            {
                Log.Warning("Order Line Item Creation Failed: Invalid order used for creation");
                return null;
            }
            if (quantity < 1)
            {
                Log.Warning("Order Line Item Creation Failed: Negative quantity used for creation");
                return null;
            }
            try
            {
                int newOrderId = order.OrderId;
                OrderLineItem newItem = new OrderLineItem(menuItem, quantity, newOrderId);

                using var connection = _dbManager.GetConnection();

                var sql = "INSERT INTO OrderLineItems (OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity) VALUES (@OrderId, @MenuItemId, @NameAtSale, @UnitPrice, @Quantity); " +
                          "SELECT last_insert_rowid();";

                newItem.LineItemId = await connection.ExecuteScalarAsync<int>(sql, newItem);

                Log.Information("Order Line Item Created: Line Item ID: {LineItemId} Order ID: {OrderId} Menu Item ID: {MenuItemId}", newItem.LineItemId, newItem.OrderId, newItem.MenuItemId);

                return newItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while creating a new order line item");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while creating a new order line item");
                return null;
            }
        }

        public async Task<Order?> CreateNewOrder()
        {
            try
            {
                Order newOrder = new Order();

                using var connection = _dbManager.GetConnection();

                var sql = "INSERT INTO Orders (CreatedAt) VALUES (@CreatedAt); " +
                          "SELECT last_insert_rowid();";

                newOrder.OrderId = await connection.ExecuteScalarAsync<int>(sql, newOrder);

                Log.Information("New Order Created: Order ID: {OrderId}", newOrder.OrderId);

                return newOrder;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected Database error while creating a new order");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while creating a new order");
                return null;
            }
        }

        public async Task DeleteOrder(int orderId) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "DELETE FROM Orders WHERE OrderId = @OrderId", new { OrderId = orderId }
                );

                Log.Information("Successful Order Deletion: Successfully deleted the order with Order ID {OrderId}", orderId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to delete the order with the Order ID {OrderId}", orderId);
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to delete the order with the Order ID {OrderId}", orderId);
                return;
            }
        }

        public async Task DeleteLineItem(int  lineItemId)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "DELETE FROM OrderLineItems WHERE LineItemId = @LineItemId", new { LineItemId = lineItemId }
                );

                Log.Information("Successful Line Item Deletion: Successfully deleted the Line Item with the Line Item ID {LineItemId}", lineItemId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to delete the Line Item with the Line Item ID {LineItemId}", lineItemId);
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to delete the Line Item with the Line Item ID {LineItemId}", lineItemId);
                return;
            }
        }

        public async Task<OrderLineItem?> UpdateOrderLineItemQuantity(int lineItemId, int quantity)
        {

            if (quantity < 1)
            {
                Log.Warning("UpdateOrderLineItemQuantity Failed: Invalid quantity used for update");
                return null;
            }
            try
            {
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE OrderLineItems SET Quantity = @Quantity WHERE LineItemId = @LineItemId",
                    new { LineItemId = lineItemId, Quantity = quantity }
                );


                var updatedItem = await GetOrderLineItemById(lineItemId);

                Log.Information("Successfully Updated Quantity: Successfully updated the quantity of the line item with the line item ID {LineItemId}", lineItemId);

                return updatedItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the quantity of the line item with the line item ID {LineItemId}", lineItemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex,"Unexpected error while attempting to update the quantity of the line item with the line item ID {LineItemId}", lineItemId);
                return null;
            }
        }

        public async Task<List<Order>> LoadOrders(bool includeLineItems = true)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string ordersQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders";

                var orders = await connection.QueryAsync<Order>(ordersQuery);

                var ordersList = orders.ToList();

                if (includeLineItems)
                {
                    foreach (var order in ordersList)
                    {
                        var lineItems = await GetOrderLineItemsByOrder(order.OrderId);
                        order.LineOrder = new ObservableCollection<OrderLineItem>(lineItems);
                    }
                }

                Log.Information("Successfully Loaded Orders: Successfully loaded {Count} orders from the database", ordersList.Count);

                return ordersList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to load orders");
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to load orders");
                return new List<Order>();
            }
        }

        public async Task<List<OrderLineItem>> LoadOrderItems()
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string orderItemsQuery = "SELECT LineItemId, OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity FROM OrderLineItems";

                var orderItems = await connection.QueryAsync<OrderLineItem>(orderItemsQuery);

                var orderItemsList = orderItems.ToList();

                Log.Information("Successfully Loaded Order Line Items: Successfully loaded {Count} order line items from the database", orderItemsList.Count);

                return orderItemsList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to load order line items from the database");
                return new List<OrderLineItem>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to load order line items from the database");
                return new List<OrderLineItem>();
            }
        }

        public async Task<Order?> FinalizeOrder(int orderId)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                   "UPDATE Orders SET IsFinalized = 1, FinalizedAt = @FinalizedAt WHERE OrderId = @OrderId",
                    new { FinalizedAt = DateTime.Now, OrderId = orderId }
                );

                Log.Information("Successfully Finalized Order: Successfully finalized the order with the order ID {OrderId}", orderId);

                return await GetOrderById(orderId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to finalize the order with the order ID {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to finalize the order with the order ID {OrderId}", orderId);
                return null;
            }
        }

        public async Task<Order?> GetOrderById(int orderId) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getOrderQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders WHERE OrderId = @OrderId";

                var retrievedOrder = await connection.QueryFirstOrDefaultAsync<Order>(getOrderQuery, new { OrderId = orderId });

                if (retrievedOrder == null)
                {
                    Log.Warning("GetOrderById Failed: Order with the Order ID {OrderId} does not exist", orderId);
                    return null;
                }
                List<OrderLineItem> retrievedOrderItems = await GetOrderLineItemsByOrder(retrievedOrder.OrderId);

                retrievedOrder.LineOrder = new ObservableCollection<OrderLineItem>(retrievedOrderItems);

                Log.Information("Successfully Retrieved Order: Successfully retrieved the order with the order ID {OrderId}", orderId);

                return retrievedOrder;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to get the order with the order ID {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to get the order with the order ID {OrderId}", orderId);
                return null;
            }
        }

        public async Task<List<OrderLineItem>> GetOrderLineItemsByOrder(int orderId)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getLineOrderItemsQuery = "SELECT LineItemId, OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity FROM OrderLineItems WHERE OrderId = @OrderId";

                var retrievedItems = await connection.QueryAsync<OrderLineItem>(getLineOrderItemsQuery, new { OrderId = orderId });

                var retrievedItemsList = retrievedItems.ToList();

                Log.Information("Successfully Retrieved Order Items: {Count} Order items were successfully retrieved for the order with the order ID {OrderId}", retrievedItemsList.Count, orderId);

                return retrievedItemsList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to get the order line items for the order with the order ID {OrderId}", orderId);
                return new List<OrderLineItem>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to get the order line items for the order with the order ID {OrderId}", orderId);
                return new List<OrderLineItem>();
            }
        }

        public async Task<OrderLineItem?> GetOrderLineItemById(int lineItemId) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getOrderLineItemQuery = "SELECT LineItemId, OrderId, MenuItemId, NameAtSale, UnitPrice, Quantity FROM OrderLineItems WHERE LineItemId = @LineItemId";

                var retrievedItem = await connection.QueryFirstOrDefaultAsync<OrderLineItem>(getOrderLineItemQuery, new { LineItemId = lineItemId });

                Log.Information("Successfully Retrieved Line Item: Successfully retrieved line item with the line item ID {LineItemId}", lineItemId);

                return retrievedItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to get the order line item with the line item ID {LineItemId}", lineItemId);
                return null;
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Unexpected error while attempting to get the order line item with the line item ID {LineItemId}", lineItemId);
                return null;
            }
        }

        public async Task<List<Order>> GetOpenOrders(bool includeLineItems = true)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getOpenOrdersQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders WHERE IsFinalized = 0";

                var openOrders = await connection.QueryAsync<Order>(getOpenOrdersQuery);

                var openOrdersList = openOrders.ToList();

                if (includeLineItems)
                {
                    foreach (var order in openOrdersList)
                    {
                        var lineItems = await GetOrderLineItemsByOrder(order.OrderId);
                        order.LineOrder = new ObservableCollection<OrderLineItem>(lineItems);
                    }
                }

                Log.Information("Successfully Loaded Orders: Successfully loaded {Count} orders from the database", openOrdersList.Count);

                return openOrdersList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to load open orders from the database");
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to load open orders from the database");
                return new List<Order>();
            }
        }

        public async Task<List<Order>> GetFinalizedOrders(bool includeLineItems = true) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getFinalizedOrdersQuery = "SELECT OrderId, CreatedAt, FinalizedAt, IsFinalized FROM Orders WHERE IsFinalized = 1";

                var finalizedOrders = await connection.QueryAsync<Order>(getFinalizedOrdersQuery);

                var finalizedOrdersList = finalizedOrders.ToList();

                if (includeLineItems)
                {
                    foreach (var order in finalizedOrdersList)
                    {
                        var lineItems = await GetOrderLineItemsByOrder(order.OrderId);
                        order.LineOrder = new ObservableCollection<OrderLineItem>(lineItems);
                    }
                }

                Log.Information("Successfully Loaded Finalized Orders: Successfully loaded {Count} finalized orders from the database", finalizedOrdersList.Count);

                return finalizedOrdersList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to load finalized orders from the database");
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to load finalized orders from the database");
                return new List<Order>();
            }
        }

    }
}
