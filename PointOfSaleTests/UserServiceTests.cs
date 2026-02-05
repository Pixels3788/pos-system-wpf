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
        public void UserCreation_ShouldReturnNewUser()
        {
            var newUser = _userService.CreateUser("Jacob", "Moore", "Pixels378@gmail.com", 1130);

            newUser.Should().NotBeNull();

        }

        [Theory]
        [InlineData(" ", "Morris", "IamMe@gmail", 1225)]
        [InlineData("Jayden", " ", "Stuffandthings@gmail", 1224)]
        [InlineData("Jay", "cub", " ", 1123)]
        [InlineData("yoyo", "mendez", "yoyo@gmail.com", 111)]
        [InlineData("Thomas", "Shereck", "tomsher@gmail.com", 1234567)]
        [InlineData("Ned", "Flanders", "NFlan@gmail.com", -4561)]
        public void UserCreation_ShouldReturnNull(string firstName, string lastName, string email, int pin)
        {
            var newUser = _userService.CreateUser(firstName, lastName, email, pin);

            newUser.Should().BeNull();
        } 

        [Fact]
        public void GetUserById_ShouldReturnUser()
        {
            var newUser = _userService.CreateUser("sean", "griggs", "motha@gmail.com", 14435);

            var newUserId = newUser.UserId;

            var result = _userService.GetUserById(newUserId);

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetUserByPin_ShouldReturnUser()
        {
            var newUser = _userService.CreateUser("Jonathan", "O'Grady", "JGrady@gmail.com", 4632);

            var result = _userService.GetUserByPin(4632);

            result.Should().NotBeNull();
        }
        [Fact]
        public void DeleteUser_ShouldReturnNull()
        {
            var newUser = _userService.CreateUser("Lucas", "Biggs", "LBiggs@gmail.com", 4566);

            var userId = newUser.UserId;

            _userService.DeleteUser(userId);

            var result = _userService.GetUserById(userId);

            result.Should().BeNull();
        }

        [Fact]
        public void UpdateUserPin_ShouldReturnUpdatedUser()
        {
            var newUser = _userService.CreateUser("Arianna", "Miller", "arimiller@gmail.com", 4312);

            var userId = newUser.UserId;

            var result = _userService.UpdateUserPin(userId, 4444);

            result.Should().NotBeNull();
            result.UserPin.Should().Be(4444);
        }

        [Fact]
        public void UpdateUserEmail_ShouldReturnUpdatedUser()
        {
            var newUser = _userService.CreateUser("David", "White", "Dwhite072@gmail.com", 2098);

            var userId = newUser.UserId;

            var result = _userService.UpdateUserEmail(userId, "Dwhite072@gmail.com");

            result.Should().NotBeNull();
            result.UserEmail.Should().Be("Dwhite072@gmail.com");
        }

        [Fact]
        public void UpdateUserRole_ShouldReturnUpdatedUser()
        {
            var newUser = _userService.CreateUser("Mulan", "Mills", "MuMills@gmai.com", 2781);

            var userId = newUser.UserId;

            var result = _userService.UpdateUserRole(userId, "Manager");

            result.Should().NotBeNull();
            result.UserRole.Should().Be("Manager");
        }

        [Fact]
        public void LoadUsers_ShouldReturnListOfUsers()
        {
            var firstNewUser = _userService.CreateUser("Milan", "Mack", "MilMack@gmail.com", 4591);
            var secondNewUser = _userService.CreateUser("Kadeem", "Carrey", "KadCar@gmail.com", 4999);

            var result = _userService.LoadUsers();

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
        public void UpdateUserPin_ShouldReturnNull(int updatedPin)
        {
            var newUser = _userService.CreateUser("jay", "ajayi", "Ajjay@gmail.com", 46543);

            var result = _userService.UpdateUserPin(newUser.UserId, updatedPin);

            result.Should().BeNull();
            newUser.UserPin.Should().Be(46543);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        public void UpdateUserEmail_ShouldReturnNull(string updatedEmail) 
        {
            var newUser = _userService.CreateUser("Tom", "Brady", "TBrad@gmail.com", 1111);

            var result = _userService.UpdateUserEmail(newUser.UserId, updatedEmail);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void UpdateUserRole_ShouldReturnNull(string updatedRole)
        {
            var newUser = _userService.CreateUser("Drew", "Brees", "Drees@gmail.com", 45653);

            var result = _userService.UpdateUserRole(newUser.UserId, updatedRole);

            result.Should().BeNull();
        }
    }
}
