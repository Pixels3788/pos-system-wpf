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
    public class OrderServiceTests
    {

        private readonly DbManager _dbManager;
        private readonly OrderService _orderService;
        private readonly MenuService _menuService;
        public OrderServiceTests()
        {
            _dbManager = new DbManager(
            "Data Source=TestDb;Mode=Memory;Cache=Shared"
            );
            _orderService = new OrderService(_dbManager);
            _menuService = new MenuService(_dbManager);
            
        }

        [Fact]
        public async Task CreateOrderLineItem_ShouldReturnOrderLineItem()
        {
            var newMenuItem = await _menuService.CreateMenuItem("test", 1.23m, "Test");
            newMenuItem.Should().NotBeNull();

            var newOrder = await _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newOrderlineItem = await _orderService.CreateOrderLineItem(newMenuItem, newOrder, 2);
            newOrderlineItem.Should().NotBeNull();
            newOrderlineItem.MenuItemId.Should().Be(newMenuItem.ItemId);
            newOrderlineItem.OrderId.Should().Be(newOrder.OrderId);
            newOrderlineItem.NameAtSale.Should().Be(newMenuItem.Name);
            newOrderlineItem.UnitPrice.Should().Be(newMenuItem.Price);
            newOrderlineItem.Quantity.Should().Be(2);

        }

        [Fact]
        public async Task CreateNewOrder_ShouldReturnNewOrder()
        {
            var result = await _orderService.CreateNewOrder();

            result.Should().NotBeNull();
        }

        

        [Fact]
        public async Task DeleteOrder_ShouldReturnNull()
        {
            var newOrder = await _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            await _orderService.DeleteOrder(newOrder.OrderId);

            var result = await _orderService.GetOrderById(newOrder.OrderId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteLineItem_ShouldReturnNull()
        {
            var newMenuItem = await _menuService.CreateMenuItem("muh", 1.25m, "test");
            newMenuItem.Should().NotBeNull();

            var newOrder = await _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newLineItem = await _orderService.CreateOrderLineItem(newMenuItem, newOrder, 3);
            newLineItem.Should().NotBeNull();

            await _orderService.DeleteLineItem(newLineItem.LineItemId);
            var result = await _orderService.GetOrderLineItemById(newLineItem.LineItemId);
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(13, 25)]
        [InlineData(45, 55)]
        public async Task UpdateOrderLineItemQuantity_ShouldReturnUpdatedItem(int quantity, int updatedQuantity)
        {
            var newMenuItem = await _menuService.CreateMenuItem("Sosa", 250.53m, "rapper");
            newMenuItem.Should().NotBeNull();

            var newOrder = await _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newLineItem = await _orderService.CreateOrderLineItem(newMenuItem, newOrder, quantity);
            newLineItem.Should().NotBeNull();

            var result = await _orderService.UpdateOrderLineItemQuantity(newLineItem.LineItemId, updatedQuantity);
            result.Should().NotBeNull();
            result.Quantity.Should().Be(updatedQuantity);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1111)]
        [InlineData(0)]
        public async Task UpdateOrderLineItemQuantity_ShouldReturnNull(int updatedQuantity)
        {
            var newMenuItem = await _menuService.CreateMenuItem("Kendrick", 1250.55m, "rapper");
            newMenuItem.Should().NotBeNull();

            var newOrder = await _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newLineItem = await _orderService.CreateOrderLineItem(newMenuItem, newOrder, 10);
            newLineItem.Should().NotBeNull();

            var result = await _orderService.UpdateOrderLineItemQuantity(newLineItem.LineItemId, updatedQuantity);
            result.Should().BeNull();
        }

        [Fact]
        public async Task FinalizeOrder_ShouldReturnFinalizedOrder()
        {
            var newOrder = await _orderService.CreateNewOrder();

            var result = await _orderService.FinalizeOrder(newOrder.OrderId);

            result.Should().NotBeNull();
            result.IsFinalized.Should().BeTrue();
        }

        [Fact]
        public async Task GetLineItemsByOrder_ShouldReturnListOfLineItems()
        {
            var firstNewMenuItem = await _menuService.CreateMenuItem("Test123", 2.57m, "testing");
            var secondNewMenuItem = await _menuService.CreateMenuItem("Testin456", 4.57m, "testing");

            var newOrder = await _orderService.CreateNewOrder();

            var firstLineItem = await _orderService.CreateOrderLineItem(firstNewMenuItem, newOrder, 3);
            var secondLineItem = await _orderService.CreateOrderLineItem(secondNewMenuItem, newOrder, 2);

            var result = await _orderService.GetOrderLineItemsByOrder(newOrder.OrderId);

            result.Select(i => i.MenuItemId).Should().Contain(new[] { firstNewMenuItem.ItemId, secondNewMenuItem.ItemId });
            result.Select(i => i.LineItemId).Should().Contain(new[] { firstLineItem.LineItemId, secondLineItem.LineItemId });
            result.Select(i => i.OrderId).Should().Contain(new[] { newOrder.OrderId });
        }

        [Fact]
        public async Task GetOpenOrders_ShouldReturnOpenOrders()
        {
            var firstOrder = await _orderService.CreateNewOrder();
            firstOrder.Should().NotBeNull();
            var secondOrder = await _orderService.CreateNewOrder();
            secondOrder.Should().NotBeNull();
            var thirdOrder = await _orderService.CreateNewOrder();
            thirdOrder.Should().NotBeNull();    

            firstOrder = await _orderService.FinalizeOrder(firstOrder.OrderId);
            firstOrder.Should().NotBeNull();

            var result = await _orderService.GetOpenOrders();
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Select(i => i.OrderId).Should().Contain(new[] { secondOrder.OrderId, thirdOrder.OrderId });

          
        }

        [Fact]
        public async Task GetFinalizedOrders_ShouldReturnOpenOrders()
        {
            var firstOrder = await _orderService.CreateNewOrder();
            firstOrder.Should().NotBeNull();

            var secondOrder = await _orderService.CreateNewOrder();
            secondOrder.Should().NotBeNull();

            var thirdOrder = await _orderService.CreateNewOrder();
            thirdOrder.Should().NotBeNull();

            firstOrder = await _orderService.FinalizeOrder(firstOrder.OrderId);
            firstOrder.Should().NotBeNull();

            secondOrder = await _orderService.FinalizeOrder(secondOrder.OrderId);

            var result = await _orderService.GetFinalizedOrders();
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Select(i => i.OrderId).Should().Contain(new[] { firstOrder.OrderId, secondOrder.OrderId });
        }


    }
}
