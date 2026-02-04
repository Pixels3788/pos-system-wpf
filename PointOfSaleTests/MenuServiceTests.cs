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
        public void MenuItemCreation_ShouldReturnNull(string name, decimal price, string category)
        {
            

            var result = _menuService.CreateMenuItem(name, price, category);


            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Coffee", 2.99, "Beverages")]
        [InlineData("Burger", 5.49, "Entrees")]
        [InlineData("Salad", 4.99, "Appetizers")]
        [InlineData("Pizza", 7.9, "Entrees")]
        public void MenuItemCreation_ShouldReturnMenuItem(string name, decimal price, string category)
        {
            
            var result = _menuService.CreateMenuItem(name, price, category);

            result.Should().NotBeNull();
            result.Name.Should().Be(name);
            result.Price.Should().Be(price);
            result.Category.Should().Be(category);  
            
        }

        [Fact]
        public void MenuLoading_ShouldReturnMenuItemsList()
        {
            var newItem = _menuService.CreateMenuItem("Burger", 1.11m, "Food");
            var newestItem = _menuService.CreateMenuItem("Cheeseburger", 1.25m, "Food");

            var result = _menuService.LoadMenuItems();

            
            result.Select(i => i.Name).Should().Contain(new[] { "Burger", "Cheeseburger" });

        }

        [Theory]
        [InlineData(1, 0.00)]
        [InlineData(1, -0.01)]
        [InlineData(1, 0.001)]
        public void MenuUpdatePrice_ShouldReturnNull(int itemId, decimal newPrice)
        {
            

            var result = _menuService.UpdateItemPrice(itemId, newPrice);

            
            result.Should().BeNull();

        }

        [Theory]
        [InlineData("hamburger", 1.10, "food", 2, 1.15)]
        [InlineData("Cheeseburger", 0.88, "Food", 4, 25.12)]
        [InlineData("Gyro", 2.50, "Food", 5, 2.25)]
        public void MenuUpdatePrice_ShouldReturnUpdatedItem(string name, decimal price, string category, int itemId, decimal newPrice)
        {
            

            var item = _menuService.CreateMenuItem(name, price, category);

            itemId = item.ItemId;

            var result = _menuService.UpdateItemPrice(itemId, newPrice);

            result.Should().NotBeNull();
            result.Price.Should().Be(newPrice);
            
        }

        [Theory]
        [InlineData ("Cheeseburger", 2.25, "Food")]
        [InlineData ("HandyJ", 45.00, "Special")]
        [InlineData ("BJ", 125.00, "Extra Special")]
        public void MenuItemDeletion_ShouldReturnVoid(string name, decimal price, string category) 
        {
            var newItem = _menuService.CreateMenuItem(name, price, category);

            newItem.Should().NotBeNull();

            var itemId = newItem.ItemId;

            _menuService.DeleteMenuItem(itemId);

            newItem = _menuService.GetItemById(itemId);

            newItem.Should().BeNull();

        }


    }
}
