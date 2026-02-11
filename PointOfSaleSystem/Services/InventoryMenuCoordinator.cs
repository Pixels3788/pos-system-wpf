using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services
{
    public class InventoryMenuCoordinator : IInventoryMenuCoordinator
    {

        private IInventoryService _inventoryService;

        private IMenuService _menuService;

        public InventoryMenuCoordinator(IInventoryService inventoryService, IMenuService menuService)
        {
            _inventoryService = inventoryService;
            _menuService = menuService;
        }

        public List<InventoryItem> ReconstructInventoryItems()
        {
            List<InventoryItem> inventoryItems = _inventoryService.LoadInventoryItems();

            foreach (InventoryItem item in inventoryItems)
            {
                MenuItem? menuItem = _menuService.GetItemById(item.MenuItemId);
                if (menuItem != null) {
                    item.MenuItem = menuItem;
                    item.MenuItemId = menuItem.ItemId;
                }
                
            }

            return inventoryItems;
        }


        public void CreateInventoryForMenuItem(string name, decimal price, string category, int startingQuantity)
        {
            if (startingQuantity < 1) return;

            MenuItem? newItem = _menuService.CreateMenuItem(name, price, category);

            if (newItem == null) return;

            InventoryItem? newInventory = _inventoryService.CreateInventoryItem(newItem, startingQuantity);
        }
    }
}
