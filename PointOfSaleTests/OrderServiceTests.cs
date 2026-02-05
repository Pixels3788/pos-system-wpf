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



    }
}
