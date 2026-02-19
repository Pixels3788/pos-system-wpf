using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

/* inventory menu coordination service that is responsible for coordinating actions between
 * the menu service and the inventory service
 */
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

        public async Task<List<InventoryItem>> ReconstructInventoryItems()
        {
            List<InventoryItem> inventoryItems = await _inventoryService.LoadInventoryItems();

            foreach (InventoryItem item in inventoryItems)
            {
                MenuItem? menuItem = await _menuService.GetItemById(item.MenuItemId);
                if (menuItem != null) {
                    item.MenuItem = menuItem;
                    item.MenuItemId = menuItem.ItemId;
                }
                
            }

            return inventoryItems;
        }


        public async void CreateInventoryForMenuItem(string name, decimal price, string category, int startingQuantity)
        {
            if (startingQuantity < 1) return;

            MenuItem? newItem = await _menuService.CreateMenuItem(name, price, category);

            if (newItem == null) return;

            InventoryItem? newInventory = await _inventoryService.CreateInventoryItem(newItem, startingQuantity);
        }
    }
}
