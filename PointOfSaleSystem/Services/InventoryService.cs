using Dapper;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using PointOfSaleSystem.Database.Interfaces;
using Serilog;
using Microsoft.Data.Sqlite;


// inventory service that is responsible for CRUD operations relating to the inventory items table of the database
namespace PointOfSaleSystem.Services
{
    public class InventoryService : IInventoryService
    {

        private IDbManager _dbManager;

        public InventoryService(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<InventoryItem?> CreateInventoryItem(MenuItem item, int quantity)
        {
            if (item == null)
            {
                Log.Warning("CreateInventoryItem failed: MenuItem is null");
                return null;
            }
            if (quantity < 1)
            {

                Log.Warning("CreateInventoryItem failed: Invalid quantity {Quantity}", quantity);
                return null;
            }

            try
            {
                InventoryItem? newInventoryItem = new InventoryItem(item, quantity);

                using var connection = _dbManager.GetConnection();



                var sql = "INSERT INTO InventoryItems (QuantityOnHand, MenuItemId) VALUES (@QuantityOnHand, @MenuItemId); " +
                          "SELECT last_insert_rowid(); ";

                newInventoryItem.InventoryItemId = await connection.ExecuteScalarAsync<int>(sql, newInventoryItem);

                Log.Information("Successful Inventory Item Creation: Successfully created an inventory item with the inventory item ID {InventoryItemId}", newInventoryItem.InventoryItemId);

                return newInventoryItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to create inventory item");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to create inventory item");
                return null;
            }
        }

        public async Task DeleteInventoryItem(InventoryItem item)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "DELETE FROM InventoryItems WHERE InventoryItemId = @InventoryItemId", new { InventoryItemId = item.InventoryItemId }
                );
                Log.Information("Successful Inventory Item Deletion: Successfully deleted the inventory item with the inventory item ID {InventoryItemId}", item.InventoryItemId);
            }
            catch (SqliteException ex) 
            {
                Log.Error(ex, "Unexpected database error while attempting to delete the inventory item with the inventory item ID {InventoryItemId}", item.InventoryItemId);
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to delete the inventory item with the inventory item ID {InventoryItemId}", item.InventoryItemId);
                return;
            }
        }

        public async Task<InventoryItem?> GetItemById(int itemId)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string itemQuery = "SELECT InventoryItemId, QuantityOnHand, MenuItemId FROM InventoryItems WHERE InventoryItemId = @itemId";

                var retrievedItem = await connection.QueryFirstOrDefaultAsync<InventoryItem>(itemQuery, new { itemId = itemId });

                if (retrievedItem == null)
                {
                    Log.Warning("Inventory Item Retrieval Failed: An inventory item with the inventory item ID {InventoryItemId} does not exist", itemId);
                    return null;
                }

                if (retrievedItem != null) {
                    Log.Information("Successful Inventory Item Retrieval: Successfully retrieved inventory item with the inventory item ID {InventoryItemId}", itemId);
                }

                return retrievedItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to retrieve an inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to retrieve an inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }
        }

        public async Task<List<InventoryItem>> LoadInventoryItems()
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string listQuery = "SELECT InventoryItemId, QuantityOnHand, MenuItemId FROM InventoryItems";

                var inventoryItems = await connection.QueryAsync<InventoryItem>(listQuery);

                var inventoryItemsList = inventoryItems.ToList();

                Log.Information("Successful Inventory Items Load: {Count} Inventory items were successfully loaded from the database", inventoryItemsList.Count);

                return inventoryItemsList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while trying to load inventory items from the database");
                return new List<InventoryItem>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while trying to load inventory items from the database");
                return new List<InventoryItem>();
            }
        }


        public async Task<InventoryItem?> GetInventoryItemByMenuItemId(int itemId) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string menuItemQuery = "SELECT InventoryItemId, QuantityOnHand, MenuItemId FROM InventoryItems WHERE MenuItemId = @itemId";

                var retrievedItem = await connection.QueryFirstOrDefaultAsync<InventoryItem>(menuItemQuery, new { itemId = itemId });

                if (retrievedItem != null)
                {
                    Log.Information("Successful Inventory Item Retrieval (By Menu Item ID): The inventory item tied to the menu item with this menu item ID {MenuItemId} was successfully retrieved", itemId);
                }
                else
                {
                    Log.Warning("Failure To Retrieve Inventory Item: Failed to retrieve an inventory item that corresponds to the Menu Item ID {MenuItemId}", itemId);
                }

                return retrievedItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while trying to fetch the inventory item that corresponds to Menu Item ID {MenuItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while trying to fetch the inventory item that corresponds to Menu Item ID {MenuItemId}", itemId);
                return null;
            }
        } 

        public async Task<InventoryItem?> DecrementInventoryItem(int itemId, int quantitySold)
        {
            try
            {
                InventoryItem? item = await GetItemById(itemId);

                if (item == null)
                {
                    Log.Warning("Decrement Inventory Item Failure: Decremenation failed because there was no corresponding item with the inventory item ID {InventoryItemId}", itemId);
                    return null;
                }
                if (item.QuantityOnHand < quantitySold)
                {
                    Log.Warning("Decrement Inventory Item Failure: Decrementation failed because an invalid quantity was used");
                    return null;
                }
                if (quantitySold <= 0)
                {
                    Log.Warning("Decrement Inventory Item Failure: Decrementation failed because an invalid quantity was used");
                    return null;
                }
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE InventoryItems SET QuantityOnHand = QuantityOnHand - @QuantitySold WHERE InventoryItemId = @InventoryItemId",
                    new { QuantitySold = quantitySold, InventoryItemId = itemId }
                );

                Log.Information("Successfully Inventory Item Decrementation: Successfully decremented the inventory item with the inventory item ID {InventoryItemID}", itemId);

                return await GetItemById(itemId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to decrement the inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            } 
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to decrement the inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }

        }

        public async Task<InventoryItem?> ChangeInventoryItemQuantity(int itemId, int newQuantity)
        {
            try
            {
                if (newQuantity <= 0)
                {
                    Log.Warning("Inventory Item Quantity Update Failure: Quantity update on the inventory item with the inventory item ID {InventoryItemId} failed due to an invalid quantity being used", itemId);
                    return null;
                }
                InventoryItem? item = await GetItemById(itemId);

                if (item == null)
                {
                    Log.Warning("Inventory Item Quantity Update Failure: Quantity update failed because an inventory item with the inventory item ID {InventoryItemID} could not be found", itemId);
                    return null;
                }
                
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE InventoryItems SET QuantityOnHand = @NewQuantity WHERE InventoryItemId = @InventoryItemId",
                    new { NewQuantity = newQuantity, InventoryItemId = itemId }
                );

                Log.Information("Inventory item {ItemId} quantity updated to {NewQuantity}", itemId, newQuantity);

                return await GetItemById(itemId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the quantity of the inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to update the quantity of the inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }
        }

        public async Task<InventoryItem?> IncrementInventoryItem(int itemId, int quantityAdded) 
        {
            try
            {
                InventoryItem? item = await GetItemById(itemId);

                if (item == null)
                {
                    Log.Warning("Inventory Item Incrementation Failure: Inventory item incrementation failed because an inventory item with the inventory item ID {InventoryItemId} could not be found", itemId);
                    return null;
                }
                if (quantityAdded < 1)
                {
                    Log.Warning("Inventory Item Incrementation Failure: Inventory item incrementation failed because an invalid quantity was used to increment");
                    return null;
                }

                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE InventoryItems SET QuantityOnHand = QuantityOnHand + @QuantityAdded WHERE InventoryItemId = @InventoryItemId",
                    new { QuantityAdded = quantityAdded, InventoryItemId = itemId }
                );

                Log.Information("Successful Inventory Item Incrementation: The inventory item with the inventory item ID {InventoryItemId} was successfully incremented", itemId);

                return await GetItemById(itemId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while trying to increment the inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while trying to increment the inventory item with the inventory item ID {InventoryItemId}", itemId);
                return null;
            }
            
        }
    }
}
