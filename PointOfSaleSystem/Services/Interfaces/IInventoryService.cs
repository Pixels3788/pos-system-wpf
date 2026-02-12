using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IInventoryService
    {
        InventoryItem? CreateInventoryItem(MenuItem item, int quantity);

        void DeleteInventoryItem(InventoryItem item);

        InventoryItem? GetItemById(int itemId);

        List<InventoryItem> LoadInventoryItems();

        InventoryItem? GetInventoryItemByMenuItemId(int itemId);

        InventoryItem? DecrementInventoryItem(int itemId, int quantitySold);

        InventoryItem? IncrementInventoryItem(int itemId, int quantityAdded);

        InventoryItem? ChangeInventoryItemQuantity(int itemId, int newQuantity);
    }
}
