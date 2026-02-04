using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;



namespace PointOfSaleSystem.Services
{
    internal class OrderInventoryCoordination
    {
        private IOrderService _orderService;
        private IInventoryService _inventoryService;

        public OrderInventoryCoordination(IOrderService orderService, IInventoryService inventoryService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
        }

        public OrderLineItem? DecrementOnCreation(MenuItem item, Order order, int quantity)
        {
            OrderLineItem? newItem = _orderService.CreateOrderLineItem(item, order, quantity);

            if (newItem == null) return null;

            InventoryItem? correspondingInventoryItem = _inventoryService.GetInventoryItemByMenuItemId(item.ItemId);

            if (correspondingInventoryItem == null) return null;

            correspondingInventoryItem = _inventoryService.DecrementInventoryItem(correspondingInventoryItem.InventoryItemId, newItem.Quantity);

            return newItem;
        }

        public void IncrementOnDeletion(OrderLineItem item) 
        {

            InventoryItem? correspondingInventoryItem = _inventoryService.GetInventoryItemByMenuItemId(item.MenuItemId);

            if (correspondingInventoryItem == null) return;

            correspondingInventoryItem = _inventoryService.IncrementInventoryItem(correspondingInventoryItem.InventoryItemId, item.Quantity);

            _orderService.DeleteLineItem(item.LineItemId);


        }
    }
}
