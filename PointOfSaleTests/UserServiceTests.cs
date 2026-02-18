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
    public class UserServiceTests
    {

        private readonly UserService _userService;
        private readonly DbManager _dbManager;

        public UserServiceTests()
        {
            _dbManager = new DbManager(
           "Data Source=TestDb;Mode=Memory;Cache=Shared"
           );
            _userService = new UserService(_dbManager);
        }

        [Fact]
        public async Task UserCreation_ShouldReturnNewUser()
        {
            var newUser = await _userService.CreateUser("Jacob", "Moore", "Pixels378@gmail.com", 1130);

            newUser.Should().NotBeNull();

        }

        [Theory]
        [InlineData(" ", "Morris", "IamMe@gmail", 1225)]
        [InlineData("Jayden", " ", "Stuffandthings@gmail", 1224)]
        [InlineData("Jay", "cub", " ", 1123)]
        [InlineData("yoyo", "mendez", "yoyo@gmail.com", 111)]
        [InlineData("Thomas", "Shereck", "tomsher@gmail.com", 1234567)]
        [InlineData("Ned", "Flanders", "NFlan@gmail.com", -4561)]
        public async Task UserCreation_ShouldReturnNull(string firstName, string lastName, string email, int pin)
        {
            var newUser = await _userService.CreateUser(firstName, lastName, email, pin);

            newUser.Should().BeNull();
        } 

        [Fact]
        public async Task GetUserById_ShouldReturnUser()
        {
            var newUser = await _userService.CreateUser("sean", "griggs", "motha@gmail.com", 14435);

            var newUserId = newUser.UserId;

            var result = await _userService.GetUserById(newUserId);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserByPin_ShouldReturnUser()
        {
            var newUser = await _userService.CreateUser("Jonathan", "O'Grady", "JGrady@gmail.com", 4632);

            var result = await _userService.GetUserByPin(4632);

            result.Should().NotBeNull();
        }
        [Fact]
        public async Task DeleteUser_ShouldReturnNull()
        {
            var newUser = await _userService.CreateUser("Lucas", "Biggs", "LBiggs@gmail.com", 4566);

            var userId = newUser.UserId;

            await _userService.DeleteUser(userId);

            var result = await _userService.GetUserById(userId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserPin_ShouldReturnUpdatedUser()
        {
            var newUser = await _userService.CreateUser("Arianna", "Miller", "arimiller@gmail.com", 4312);

            var userId = newUser.UserId;

            var result = await _userService.UpdateUserPin(userId, 4444);

            result.Should().NotBeNull();
            result.UserPin.Should().Be(4444);
        }

        [Fact]
        public async Task UpdateUserEmail_ShouldReturnUpdatedUser()
        {
            var newUser = await _userService.CreateUser("David", "White", "Dwhite072@gmail.com", 2098);

            var userId = newUser.UserId;

            var result = await _userService.UpdateUserEmail(userId, "Dwhite072@gmail.com");

            result.Should().NotBeNull();
            result.UserEmail.Should().Be("Dwhite072@gmail.com");
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnUpdatedUser()
        {
            var newUser = await _userService.CreateUser("Mulan", "Mills", "MuMills@gmai.com", 2781);

            var userId = newUser.UserId;

            var result = await _userService.UpdateUserRole(userId, "Manager");

            result.Should().NotBeNull();
            result.UserRole.Should().Be("Manager");
        }

        [Fact]
        public async Task LoadUsers_ShouldReturnListOfUsers()
        {
            var firstNewUser = await _userService.CreateUser("Milan", "Mack", "MilMack@gmail.com", 4591);
            var secondNewUser = await _userService.CreateUser("Kadeem", "Carrey", "KadCar@gmail.com", 4999);

            var result = await _userService.LoadUsers();

            result.Should().NotBeNull();
            result.Select(i => i.UserEmail).Should().Contain(new[] { "MilMack@gmail.com", "KadCar@gmail.com" });
            result.Select(i => i.FirstName).Should().Contain(new[] { "Milan", "Kadeem" });
            result.Select(i => i.LastName).Should().Contain(new[] { "Mack", "Carrey" });
            result.Select(i => i.UserPin).Should().Contain(new[] { 4591, 4999 });
        }

        [Theory]
        [InlineData(-1245)]
        [InlineData(1761761)]
        [InlineData(12)]
        public async Task UpdateUserPin_ShouldReturnNull(int updatedPin)
        {
            var newUser = await _userService.CreateUser("jay", "ajayi", "Ajjay@gmail.com", 46543);

            var result = await _userService.UpdateUserPin(newUser.UserId, updatedPin);

            result.Should().BeNull();
            newUser.UserPin.Should().Be(46543);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        public async Task UpdateUserEmail_ShouldReturnNull(string updatedEmail) 
        {
            var newUser = await _userService.CreateUser("Tom", "Brady", "TBrad@gmail.com", 1111);

            var result = await _userService.UpdateUserEmail(newUser.UserId, updatedEmail);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUserRole_ShouldReturnNull(string updatedRole)
        {
            var newUser = await _userService.CreateUser("Drew", "Brees", "Drees@gmail.com", 45653);

            var result = await _userService.UpdateUserRole(newUser.UserId, updatedRole);

            result.Should().BeNull();
        }
    }
}
