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
        public void CreateOrderLineItem_ShouldReturnOrderLineItem()
        {
            var newMenuItem = _menuService.CreateMenuItem("test", 1.23m, "Test");
            newMenuItem.Should().NotBeNull();

            var newOrder = _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newOrderlineItem = _orderService.CreateOrderLineItem(newMenuItem, newOrder, 2);
            newOrderlineItem.Should().NotBeNull();
            newOrderlineItem.MenuItemId.Should().Be(newMenuItem.ItemId);
            newOrderlineItem.OrderId.Should().Be(newOrder.OrderId);
            newOrderlineItem.NameAtSale.Should().Be(newMenuItem.Name);
            newOrderlineItem.UnitPrice.Should().Be(newMenuItem.Price);
            newOrderlineItem.Quantity.Should().Be(2);

        }

        [Fact]
        public void CreateNewOrder_ShouldReturnNewOrder()
        {
            var result = _orderService.CreateNewOrder();

            result.Should().NotBeNull();
        }

        

        [Fact]
        public void DeleteOrder_ShouldReturnNull()
        {
            var newOrder = _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            _orderService.DeleteOrder(newOrder.OrderId);

            var result = _orderService.GetOrderById(newOrder.OrderId);
            result.Should().BeNull();
        }

        [Fact]
        public void DeleteLineItem_ShouldReturnNull()
        {
            var newMenuItem = _menuService.CreateMenuItem("muh", 1.25m, "test");
            newMenuItem.Should().NotBeNull();

            var newOrder = _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newLineItem = _orderService.CreateOrderLineItem(newMenuItem, newOrder, 3);
            newLineItem.Should().NotBeNull();

            _orderService.DeleteLineItem(newLineItem.LineItemId);
            var result = _orderService.GetOrderLineItemById(newLineItem.LineItemId);
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(13, 25)]
        [InlineData(45, 55)]
        public void UpdateOrderLineItemQuantity_ShouldReturnUpdatedItem(int quantity, int updatedQuantity)
        {
            var newMenuItem = _menuService.CreateMenuItem("Sosa", 250.53m, "rapper");
            newMenuItem.Should().NotBeNull();

            var newOrder = _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newLineItem = _orderService.CreateOrderLineItem(newMenuItem, newOrder, quantity);
            newLineItem.Should().NotBeNull();

            var result = _orderService.UpdateOrderLineItemQuantity(newLineItem.LineItemId, updatedQuantity);
            result.Should().NotBeNull();
            result.Quantity.Should().Be(updatedQuantity);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1111)]
        [InlineData(0)]
        public void UpdateOrderLineItemQuantity_ShouldReturnNull(int updatedQuantity)
        {
            var newMenuItem = _menuService.CreateMenuItem("Kendrick", 1250.55m, "rapper");
            newMenuItem.Should().NotBeNull();

            var newOrder = _orderService.CreateNewOrder();
            newOrder.Should().NotBeNull();

            var newLineItem = _orderService.CreateOrderLineItem(newMenuItem, newOrder, 10);
            newLineItem.Should().NotBeNull();

            var result = _orderService.UpdateOrderLineItemQuantity(newLineItem.LineItemId, updatedQuantity);
            result.Should().BeNull();
        }

        [Fact]
        public void FinalizeOrder_ShouldReturnFinalizedOrder()
        {
            var newOrder = _orderService.CreateNewOrder();

            var result = _orderService.FinalizeOrder(newOrder.OrderId);

            result.Should().NotBeNull();
            result.IsFinalized.Should().BeTrue();
        }

        [Fact]
        public void GetLineItemsByOrder_ShouldReturnListOfLineItems()
        {
            var firstNewMenuItem = _menuService.CreateMenuItem("Test123", 2.57m, "testing");
            var secondNewMenuItem = _menuService.CreateMenuItem("Testin456", 4.57m, "testing");

            var newOrder = _orderService.CreateNewOrder();

            var firstLineItem = _orderService.CreateOrderLineItem(firstNewMenuItem, newOrder, 3);
            var secondLineItem = _orderService.CreateOrderLineItem(secondNewMenuItem, newOrder, 2);

            var result = _orderService.GetOrderLineItemsByOrder(newOrder.OrderId);

            result.Select(i => i.MenuItemId).Should().Contain(new[] { firstNewMenuItem.ItemId, secondNewMenuItem.ItemId });
            result.Select(i => i.LineItemId).Should().Contain(new[] { firstLineItem.LineItemId, secondLineItem.LineItemId });
            result.Select(i => i.OrderId).Should().Contain(new[] { newOrder.OrderId });
        }

        [Fact]
        public void GetOpenOrders_ShouldReturnOpenOrders()
        {
            var firstOrder = _orderService.CreateNewOrder();
            firstOrder.Should().NotBeNull();
            var secondOrder = _orderService.CreateNewOrder();
            secondOrder.Should().NotBeNull();
            var thirdOrder = _orderService.CreateNewOrder();
            thirdOrder.Should().NotBeNull();    

            firstOrder = _orderService.FinalizeOrder(firstOrder.OrderId);
            firstOrder.Should().NotBeNull();

            var result = _orderService.GetOpenOrders();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Select(i => i.OrderId).Should().Contain(new[] { secondOrder.OrderId, thirdOrder.OrderId });

          
        }

        [Fact]
        public void GetFinalizedOrders_ShouldReturnOpenOrders()
        {
            var firstOrder = _orderService.CreateNewOrder();
            firstOrder.Should().NotBeNull();

            var secondOrder = _orderService.CreateNewOrder();
            secondOrder.Should().NotBeNull();

            var thirdOrder = _orderService.CreateNewOrder();
            thirdOrder.Should().NotBeNull();

            firstOrder = _orderService.FinalizeOrder(firstOrder.OrderId);
            firstOrder.Should().NotBeNull();

            secondOrder = _orderService.FinalizeOrder(secondOrder.OrderId);

            var result = _orderService.GetFinalizedOrders();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Select(i => i.OrderId).Should().Contain(new[] { firstOrder.OrderId, secondOrder.OrderId });
        }


    }
}
