using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using PointOfSaleSystem.Models;
using System.ComponentModel;
using PointOfSaleSystem.Database;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Linq;
using PointOfSaleSystem.Database.Interfaces;
using Serilog;

namespace PointOfSaleSystem.Services
{
    public class MenuService : IMenuService
    {

        private IDbManager _dbManager;

        public MenuService(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }
       

        public async Task<MenuItem?> CreateMenuItem(string name, decimal price, string category)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Warning("Create Menu Item Failure: Menu item creation failed due to an invalid name being used");
                return null;
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                Log.Warning("Create Menu Item Failure: Menu item creation failed due to an invalid category being used");
                return null;
            }
            if (price < 0.01m)
            {
                Log.Warning("Create Menu Item Failure: Menu item creation failed due to an invalid price being used");
                return null;
            }

            try
            {
                MenuItem newItem = new MenuItem(name, price, category);
                using var connection = _dbManager.GetConnection();

                var sql = "INSERT INTO MenuItems (Name, Price, Category) VALUES (@Name, @Price, @Category); " +
                          "SELECT last_insert_rowid();";

                newItem.ItemId = await connection.ExecuteScalarAsync<int>(sql, newItem);

                Log.Information("New Menu Item Creation: Successfully created new menu item with the menu item ID {MenuItemId}", newItem.ItemId);

                return newItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to create a new menu item");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to create a new menu item");
                return null;
            }
            
        }

        public async Task DeleteMenuItem(int itemId)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                MenuItem? item = await GetItemById(itemId);
                if (item != null)
                {
                    Log.Information("Successful Menu Item Deletion: The menu item with the menu item ID {MenuItemId} was successfully deleted", itemId);
                }
                else
                {
                    Log.Warning("Menu Item Deletion Failure: Could not find a menu item with the menu item ID {MenuItemId}", itemId);
                    return;
                }

                await connection.ExecuteAsync(
                    "DELETE FROM MenuItems WHERE ItemId = @ItemId", new { ItemId = itemId }
                );
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to delete the menu item with the menu item ID {MenuItemId}", itemId);
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to delete the menu item with the menu item ID {MenuItemId}", itemId);
                return;
            }
        }

        public async Task<List<MenuItem>> LoadMenuItems()
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string menuQuery = "SELECT ItemId, Name, Price, Category FROM MenuItems";

                var menuItems = await connection.QueryAsync<MenuItem>(menuQuery);

                var menuItemsList = menuItems.ToList();

                Log.Information("Successful Menu Items Load: Successfully loaded {Count} menu items from the database", menuItemsList.Count);

                return menuItemsList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to load the menu items from the database");
                return new List<MenuItem>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to load the menu items from the database");
                return new List<MenuItem>();
            }

        }

        public async Task<MenuItem?> GetItemById(int itemId)
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string itemQuery = "SELECT ItemId, Name, Price, Category FROM MenuItems WHERE ItemId = @ItemId";

                var retrievedItem = await connection.QueryFirstOrDefaultAsync<MenuItem>(itemQuery, new { ItemId = itemId });

                Log.Information("Successful Menu Item Retrieval: Successfully retrieved menu item with the menu item ID {MenuItemId}", itemId);

                return retrievedItem;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to fetch the menu item with the menu item ID {MenuItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to fetch the menu item with the menu item ID {MenuItemId}", itemId);
                return null;
            }
        }

        public async Task<MenuItem?> UpdateItemPrice(int itemId, decimal newPrice)
        {
            try
            {
                if (newPrice < 0.01m) return null;

                using var connection = _dbManager.GetConnection();

                var updatedItem = await GetItemById(itemId);

                if (updatedItem != null)
                {
                    Log.Information("Successful Menu Item Price Update: Successfully updated the price of the menu item with the menu item ID {MenuItemId}", itemId);
                } 
                else
                {
                    Log.Warning("Menu Item Price Update Failure: Menu item pricing update failed because a menu item with the menu item ID {MenuItemId} could not be found", itemId);
                    return null;
                }

                await connection.ExecuteAsync(
                    "UPDATE MenuItems SET Price = @NewPrice WHERE ItemId = @ItemId",
                    new { NewPrice = newPrice, ItemId = itemId }
                );

                return await GetItemById(itemId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the price of the menu item with the menu item ID {MenuItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to update the price of the menu item with the menu item ID {MenuItemId}", itemId);
                return null;
            }
        }

        public async Task<MenuItem?> UpdateItemName(int itemId, string newName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newName)) return null;

                using var connection = _dbManager.GetConnection();

                var updatedItem = GetItemById(itemId);

                if (updatedItem != null)
                {
                    Log.Information("Successful Menu Item Name Update: Successfully updated the name of the menu item with the menu item ID {MenuItemId}", itemId);
                }
                else
                {
                    Log.Warning("Menu Item Name Update Failure: Could not update the name of the menu item because {MenuItemId} menu item ID does not exist", itemId);
                }

                await connection.ExecuteAsync(
                    "UPDATE MenuItems SET Name = @NewName WHERE ItemId = @ItemId",
                    new { NewName = newName, ItemId = itemId }
                );

                return await GetItemById(itemId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the name of the menu item with the menu item ID {MenuItemId}", itemId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the name of the menu item with the menu item ID {MenuItemId}", itemId);
                return null;
            }
        }
    }
}
