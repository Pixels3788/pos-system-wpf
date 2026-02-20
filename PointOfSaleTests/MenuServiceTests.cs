using Xunit;
using Moq;
using FluentAssertions;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Services.Interfaces;

namespace PointOfSaleTests
{
    public class MenuServiceTests : IDisposable
    {

        private readonly DbManager _dbManager;
        private readonly MenuService _menuService;

        public MenuServiceTests()
        {
            _dbManager = new DbManager(
            "Data Source=TestDb;Mode=Memory;Cache=Shared"
            );
            _menuService = new MenuService(_dbManager);
        }

        public void Dispose()
        {
            _dbManager.Dispose();
        }
        
        [Theory]
        [InlineData(" ", 5.99, "Beverages")]
        [InlineData("Burger", 0.00, "Beverages")]
        [InlineData("Pizza", 8.99, " ")]
        public async Task MenuItemCreation_ShouldReturnNull(string name, decimal price, string category)
        {
            

            var result = await _menuService.CreateMenuItem(name, price, category);


            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Coffee", 2.99, "Beverages")]
        [InlineData("Burger", 5.49, "Entrees")]
        [InlineData("Salad", 4.99, "Appetizers")]
        [InlineData("Pizza", 7.9, "Entrees")]
        public async Task MenuItemCreation_ShouldReturnMenuItem(string name, decimal price, string category)
        {
            
            var result = await _menuService.CreateMenuItem(name, price, category);

            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Price.Should().Be(price);
            result.Category.Should().Be(category);  
            
        }

        [Fact]
        public async Task MenuLoading_ShouldReturnMenuItemsList()
        {
            var newItem = await _menuService.CreateMenuItem("Burger", 1.11m, "Food");
            var newestItem = await _menuService.CreateMenuItem("Cheeseburger", 1.25m, "Food");

            var result = await _menuService.LoadMenuItems();

            
            result.Select(i => i.Name).Should().Contain(new[] { "Burger", "Cheeseburger" });

        }

        [Theory]
        [InlineData(1, 0.00)]
        [InlineData(1, -0.01)]
        [InlineData(1, 0.001)]
        public async Task MenuUpdatePrice_ShouldReturnNull(int itemId, decimal newPrice)
        {
            

            var result = await _menuService.UpdateItemPrice(itemId, newPrice);

            
            result.Should().BeNull();

        }

        [Theory]
        [InlineData("hamburger", 1.10, "food", 2, 1.15)]
        [InlineData("Cheeseburger", 0.88, "Food", 4, 25.12)]
        [InlineData("Gyro", 2.50, "Food", 5, 2.25)]
        public async Task MenuUpdatePrice_ShouldReturnUpdatedItem(string name, decimal price, string category, int itemId, decimal newPrice)
        {
            

            var item = await _menuService.CreateMenuItem(name, price, category);

            itemId = item.ItemId;

            var result = await _menuService.UpdateItemPrice(itemId, newPrice);

            result.Should().NotBeNull();
            result.Price.Should().Be(newPrice);
            
        }

        [Theory]
        [InlineData ("Cheeseburger", 2.25, "Food")]
        [InlineData ("Hmm", 45.00, "Special")]
        [InlineData ("hhm", 125.00, "Extra Special")]
        public async Task MenuItemDeletion_ShouldReturnVoid(string name, decimal price, string category) 
        {
            var newItem = await _menuService.CreateMenuItem(name, price, category);

            newItem.Should().NotBeNull();

            var itemId = newItem.ItemId;

            _menuService.DeleteMenuItem(itemId);

            newItem = await _menuService.GetItemById(itemId);

            newItem.Should().BeNull();

        }


    }
}
