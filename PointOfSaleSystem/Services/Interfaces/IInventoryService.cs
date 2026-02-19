using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

// public interface for the inventory service
namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryItem?> CreateInventoryItem(MenuItem item, int quantity);

        Task DeleteInventoryItem(InventoryItem item);

        Task<InventoryItem?> GetItemById(int itemId);

        Task<List<InventoryItem>> LoadInventoryItems();

        Task<InventoryItem?> GetInventoryItemByMenuItemId(int itemId);

        Task<InventoryItem?> DecrementInventoryItem(int itemId, int quantitySold);

        Task<InventoryItem?> IncrementInventoryItem(int itemId, int quantityAdded);

        Task<InventoryItem?> ChangeInventoryItemQuantity(int itemId, int newQuantity);
    }
}
