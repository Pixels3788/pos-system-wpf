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

namespace PointOfSaleSystem.Services
{
    public class MenuService : IMenuService
    {

        private IDbManager _dbManager;

        public MenuService(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }
       

        public MenuItem? CreateMenuItem(string name, decimal price, string category)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (string.IsNullOrWhiteSpace(category)) return null;
            if (price < 0.01m) return null;

            MenuItem newItem = new MenuItem(name, price, category);
            using var connection = _dbManager.GetConnection();

            var sql = "INSERT INTO MenuItems (Name, Price, Category) VALUES (@Name, @Price, @Category); " +
                      "SELECT last_insert_rowid();";

            newItem.ItemId = connection.ExecuteScalar<int>(sql, newItem);
            return newItem;
            
        }

        public void DeleteMenuItem(int itemId)
        {
            using var connection = _dbManager.GetConnection();

            MenuItem? item = GetItemById(itemId);
            if (item == null) return;

            connection.Execute(
                "DELETE FROM MenuItems WHERE ItemId = @ItemId", new {ItemId = itemId}
            );
        }

        public List<MenuItem> LoadMenuItems()
        {
            using var connection = _dbManager.GetConnection();

            string menuQuery = "SELECT ItemId, Name, Price, Category FROM MenuItems";

            var menuItems = connection.Query<MenuItem>( menuQuery ).ToList();

            return menuItems;

        }

        public MenuItem? GetItemById(int itemId)
        {
            using var connection = _dbManager.GetConnection();

            string itemQuery = "SELECT ItemId, Name, Price, Category FROM MenuItems WHERE ItemId = @ItemId";

            var retrievedItem = connection.QueryFirstOrDefault<MenuItem>(itemQuery, new { ItemId = itemId });

            return retrievedItem;
        }

        public MenuItem? UpdateItemPrice(int itemId, decimal newPrice)
        {

            if (newPrice < 0.01m) return null;

            using var connection = _dbManager.GetConnection();

            var updatedItem = GetItemById(itemId);

            if (updatedItem == null) return null;

            connection.Execute(
                "UPDATE MenuItems SET Price = @NewPrice WHERE ItemId = @ItemId",
                new {NewPrice = newPrice, ItemId = itemId}
            );



            return GetItemById(itemId);
        }
    }
}
