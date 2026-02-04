using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Services.Interfaces;

namespace PointOfSaleTests
{
    public class InventoryServiceTests : IDisposable
    {

        private readonly DbManager _dbManager;
        private readonly InventoryService _inventoryService;
        private readonly MenuService _menuService;

        public InventoryServiceTests()
        {
            _dbManager = new DbManager(
            "Data Source=TestDb;Mode=Memory;Cache=Shared"
            );
            _inventoryService = new InventoryService( _dbManager );
            _menuService = new MenuService( _dbManager );
        }

        public void Dispose()
        {
            _dbManager.Dispose();
        }

        [Theory]
        [InlineData (12, "Burger", 2.50, "Food")]
        [InlineData (132, "Cheeseburger", 3.50, "Food")]
        [InlineData (220, "Pizza", 12.99, "Food")]
        public void InventoryServiceItemCreation_ShouldReturnItem(int quantity, string name, decimal price, string category)
        {
            var testMenuItem = _menuService.CreateMenuItem(name, price, category);

            testMenuItem.Should().NotBeNull();
            testMenuItem.Name.Should().Be(name);
            testMenuItem.Price.Should().Be(price);
            testMenuItem.Category.Should().Be(category);

            var testInventoryItem = _inventoryService.CreateInventoryItem(testMenuItem, quantity);

            testInventoryItem.Should().NotBeNull();
            testInventoryItem.MenuItem.ItemId.Should().Be(testMenuItem.ItemId);
            testInventoryItem.QuantityOnHand.Should().Be(quantity);
        }

        [Theory]
        [InlineData (-1, "Burger", 3.75, "Food")]
        [InlineData (0, "Cheeseburger", 5.25, "Food")]
        [InlineData (-45, "Coffee", 2.75, "Beverage")]
        public void InventoryServiceItemCreation_ShouldReturnNull(int quantity, string name, decimal price, string category)
        {
            var testMenuItem = _menuService.CreateMenuItem(name, price, category);

            testMenuItem.Should().NotBeNull();
            testMenuItem.Name.Should().Be(name);
            testMenuItem.Price.Should().Be(price);
            testMenuItem.Category.Should().Be(category);

            var testInventoryItem = _inventoryService.CreateInventoryItem(testMenuItem, quantity);

            testInventoryItem.Should().BeNull();
        }

        [Fact]
        public void LoadInventoryTest_ShouldReturnInventoryItemsList()
        {
            var newMenuItem = _menuService.CreateMenuItem("Fries", 1.11m, "Food");
            var newMenuItem2 = _menuService.CreateMenuItem("Soda", 2.50m, "Beverage");

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, 90);
            var newInventoryItem2 = _inventoryService.CreateInventoryItem(newMenuItem2, 230);

            var result = _inventoryService.LoadInventoryItems();

            result.Select(i => i.QuantityOnHand).Should().Contain(new[] { 90, 230 });

        }

        [Fact]
        public void DeleteInventoryItem_ShouldReturnNull()
        {
            var newMenuItem = _menuService.CreateMenuItem("Ham", 4.59m, "Food");

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, 100);

            var newInvItemId = newInventoryItem.InventoryItemId;

            _inventoryService.DeleteInventoryItem(newInventoryItem);

            var result = _inventoryService.GetItemById(newInvItemId);

            result.Should().BeNull();

        }

        [Fact]
        public void InventoryGetByMenuItemId_ShouldReturnInventoryItem()
        {
            var newMenuItem = _menuService.CreateMenuItem("Coke", 2.66m, "Beverage");

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, 124);

            var result = _inventoryService.GetInventoryItemByMenuItemId(newMenuItem.ItemId);

            result.Should().NotBeNull();
            result.InventoryItemId.Should().Be(newInventoryItem.InventoryItemId);
            result.MenuItemId.Should().Be(newMenuItem.ItemId);
            result.QuantityOnHand.Should().Be(124);
        }

        [Theory]
        [InlineData("Pepsi", 2.55, "Beverage", 12, 25)]
        [InlineData("Codeine", 45.55, "Special Menu", 45, 55)]
        [InlineData("Hydrocodone", 75.55, "Extra Special Menu", 200, 455)]
        public void IncrementInventoryItem_ReturnsUpdatedItem(string name, decimal price, string category, int quantity, int quantityAdded)
        {
            var newMenuItem = _menuService.CreateMenuItem(name, price, category);

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, quantity);

            var result = _inventoryService.IncrementInventoryItem(newInventoryItem.InventoryItemId, quantityAdded);

            result.Should().NotBeNull();
            result.InventoryItemId.Should().Be(newInventoryItem.InventoryItemId);
            result.QuantityOnHand.Should().Be(quantity + quantityAdded);
        }

        [Theory]
        [InlineData("Special Sauce", 450.55, "Sauces", 8, -1)]
        [InlineData("Extra Special Sauce", 455.56, "Sauces", 12, 0)]
        [InlineData("Extra Extra Special Sauce", 7500.55, "Sauces", 15, -455)]
        public void IncrementInventoryItem_ReturnsNull(string name, decimal price, string category, int quantity, int quantityAdded)
        {
            var newMenuItem = _menuService.CreateMenuItem(name, price, category);

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, quantity);

            var result = _inventoryService.IncrementInventoryItem(newInventoryItem.InventoryItemId, quantityAdded);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Mountain Dew", 2.00, "Beverages", 20, 18)]
        [InlineData("Dr. Pepper", 2.55, "Beverages", 200, 155)]
        [InlineData("Mr. Pibb", 3.55, "Beverages", 455, 287)]
        public void DecrementInventoryItem_ReturnsUpdatedItem(string name, decimal price, string category, int quantity, int quantitySold)
        {
            var newMenuItem = _menuService.CreateMenuItem(name, price, category);

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, quantity);

            var result = _inventoryService.DecrementInventoryItem(newInventoryItem.InventoryItemId, quantitySold);
            
            result.Should().NotBeNull();
            result.InventoryItemId.Should().Be(newInventoryItem.InventoryItemId);
            result.QuantityOnHand.Should().Be(quantity - quantitySold);
        }

        [Theory]
        [InlineData("Dr. Thunder", 3.20, "Beverages", 155, 200)]
        [InlineData("Water", 0.50, "Beverages", 1000, 0)]
        [InlineData("Sparkling Water", 2.00, "Beverages", 10, -25)]
        public void DecrementInventoryItem_ReturnsNull(string name, decimal price, string category, int quantity, int quantitySold)
        {
            var newMenuItem = _menuService.CreateMenuItem(name, price, category);

            var newInventoryItem = _inventoryService.CreateInventoryItem(newMenuItem, quantity);

            var result = _inventoryService.DecrementInventoryItem(newInventoryItem.InventoryItemId, quantitySold);

            result.Should().BeNull();
        }
    }
}
