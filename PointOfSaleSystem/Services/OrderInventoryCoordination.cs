using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;


// Order inventory coordination service that is responsible for coordinating CRUD operations between the order and inventory services
namespace PointOfSaleSystem.Services
{
    public class OrderInventoryCoordination : IOrderInventoryCoordination
    {
        private IOrderService _orderService;
        private IInventoryService _inventoryService;

        public OrderInventoryCoordination(IOrderService orderService, IInventoryService inventoryService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
        }

        public async Task<OrderLineItem?> DecrementOnCreation(MenuItem item, Order order, int quantity)
        {
            OrderLineItem? newItem = null;

            try
            {
                
                newItem = await _orderService.CreateOrderLineItem(item, order, quantity);

                if (newItem == null)
                {
                    Log.Warning("DecrementOnCreation failed: Could not create order line item for menu item {MenuItemId}", item.ItemId);
                    return null;
                }

                
                InventoryItem? correspondingInventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(item.ItemId);

                if (correspondingInventoryItem == null)
                {
                   
                    Log.Warning("DecrementOnCreation: Order line item {LineItemId} created but no inventory found for menu item {MenuItemId}. Inventory not decremented.",
                        newItem.LineItemId, item.ItemId);
                    return newItem;  
                }

                
                correspondingInventoryItem = await _inventoryService.DecrementInventoryItem(
                    correspondingInventoryItem.InventoryItemId, newItem.Quantity);

                if (correspondingInventoryItem == null)
                {
                    
                    Log.Warning("DecrementOnCreation: Order line item {LineItemId} created but inventory decrement FAILED for menu item {MenuItemId}. DATA MAY BE INCONSISTENT.",
                        newItem.LineItemId, item.ItemId);
                }

                return newItem;
            }
            catch (Exception ex)
            {
                
                if (newItem != null)
                {
                    Log.Error(ex, "DecrementOnCreation crashed AFTER creating order line item {LineItemId}. Inventory may not have been decremented. DATA MAY BE INCONSISTENT.",
                        newItem.LineItemId);
                }
                else
                {
                    Log.Error(ex, "DecrementOnCreation crashed before creating order line item for menu item {MenuItemId}",
                        item.ItemId);
                }
                return null;
            }
        }

        public async Task IncrementOnDeletion(OrderLineItem item) 
        {
            bool inventoryIncremented = false;
            try
            {
                InventoryItem? correspondingInventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(item.MenuItemId);

                if (correspondingInventoryItem == null)
                {
                    Log.Warning("IncrementOnDeletion: No inventory found for menu item on: No inventory found for menu item {MenuItemId}. Proceeding with line item deletion only.", item.MenuItemId);
                    return;
                }
                else 
                {
                    correspondingInventoryItem = await _inventoryService.IncrementInventoryItem(correspondingInventoryItem.InventoryItemId, item.Quantity);

                    inventoryIncremented = correspondingInventoryItem != null;

                    if (!inventoryIncremented)
                    {
                        Log.Warning("IncrementOnDeletion: Inventory increment failed for menu item {MenuItemId}. Proceeding with line item deletion.",
                                    item.MenuItemId);
                    }

                    await _orderService.DeleteLineItem(item.LineItemId);
                }
            }
            catch (Exception ex)
            {
                if (inventoryIncremented)
                {
                    Log.Error(ex, "IncrementOnDeletion crashed AFTER incrementing inventory for menu item {MenuItemId}. Line item {LineItemId} may NOT have been deleted. DATA MAY BE INCONSISTENT.",
                        item.MenuItemId, item.LineItemId);
                }
                else
                {
                    Log.Error(ex, "IncrementOnDeletion crashed for line item {LineItemId}",
                        item.LineItemId);
                }
            }



        }

        public async Task DecrementOnQuantityChanged(OrderLineItem item, int quantity)
        {
            try
            {
                InventoryItem? correspondingInventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(item.MenuItemId);

                if (correspondingInventoryItem == null)
                {
                    Log.Warning("DecrementOnQuantityChanged: No inventory found for menu item {MenuItemId}",
                        item.MenuItemId);
                    return;
                }

                var result = await _inventoryService.DecrementInventoryItem(
                    correspondingInventoryItem.InventoryItemId, quantity);

                if (result == null)
                {
                    Log.Warning("DecrementOnQuantityChanged: Decrement failed for inventory item {InventoryItemId}",
                        correspondingInventoryItem.InventoryItemId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DecrementOnQuantityChanged crashed for line item {LineItemId}, quantity {Quantity}",
                    item.LineItemId, quantity);
            }
        }
    }
}
