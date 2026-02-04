using Dapper;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services
{
    public class InventoryService : IInventoryService
    {

        private DbManager _dbManager;

        public InventoryService(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public InventoryItem? CreateInventoryItem(MenuItem item, int quantity)
        {
            if (item == null) return null;
            if (quantity < 1 ) return null;

            InventoryItem? newInventoryItem = new InventoryItem(item, quantity);

            using var connection = _dbManager.GetConnection();

            

            var sql = "INSERT INTO InventoryItems (QuantityOnHand, MenuItemId) VALUES (@QuantityOnHand, @MenuItemId); " +
                      "SELECT last_insert_rowid(); ";

            newInventoryItem.InventoryItemId = connection.ExecuteScalar<int>(sql, newInventoryItem);
            return newInventoryItem;
        }

        public void DeleteInventoryItem(InventoryItem item)
        {
            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "DELETE FROM InventoryItems WHERE InventoryItemId = @InventoryItemId", new {InventoryItemId = item.InventoryItemId }
            );
        }

        public InventoryItem? GetItemById(int itemId)
        {
            using var connection = _dbManager.GetConnection();

            string itemQuery = "SELECT InventoryItemId, QuantityOnHand, MenuItemId FROM InventoryItems WHERE InventoryItemId = @itemId";

            var retrievedItem = connection.QueryFirstOrDefault<InventoryItem>(itemQuery, new {itemId = itemId});

            if (retrievedItem == null) return null;

            return retrievedItem;

        }

        public List<InventoryItem> LoadInventoryItems()
        {
            using var connection = _dbManager.GetConnection();

            string listQuery = "SELECT InventoryItemId, QuantityOnHand, MenuItemId FROM InventoryItems";

            var inventoryItems = connection.Query<InventoryItem>(listQuery).ToList();

            return inventoryItems;
        }


        public InventoryItem? GetInventoryItemByMenuItemId(int itemId) 
        {
            using var connection = _dbManager.GetConnection();

            string menuItemQuery = "SELECT InventoryItemId, QuantityOnHand, MenuItemId FROM InventoryItems WHERE MenuItemId = @itemId";

            var retrievedItem = connection.QueryFirstOrDefault<InventoryItem>(menuItemQuery, new {itemId = itemId});

            return retrievedItem;
        } 

        public InventoryItem? DecrementInventoryItem(int itemId, int quantitySold)
        {
            
            InventoryItem? item = GetItemById(itemId);

            if (item == null) return null;
            if (item.QuantityOnHand <  quantitySold) return null;
            if (quantitySold <= 0) return null;
            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "UPDATE InventoryItems SET QuantityOnHand = QuantityOnHand - @QuantitySold WHERE InventoryItemId = @InventoryItemId",
                new {QuantitySold = quantitySold, InventoryItemId = itemId}
            );
            
            

            return GetItemById(itemId);

        }

        public InventoryItem? IncrementInventoryItem(int itemId, int quantityAdded) 
        {
            InventoryItem? item = GetItemById(itemId);

            if (item == null) return null;
            if (quantityAdded < 1) return null;

            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "UPDATE InventoryItems SET QuantityOnHand = QuantityOnHand + @QuantityAdded WHERE InventoryItemId = @InventoryItemId",
                new {QuantityAdded  = quantityAdded, InventoryItemId = itemId}
            );

            return GetItemById(itemId);
            
        }
    }
}
