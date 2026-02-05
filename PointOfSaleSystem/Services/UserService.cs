using PointOfSaleSystem.Database;
using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Models;

using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using PointOfSaleSystem.Database.Interfaces;

namespace PointOfSaleSystem.Services
{
    public class UserService : IUserService
    {
        private IDbManager _dbManager;

        public UserService(IDbManager dbManager)
        {
            _dbManager = dbManager; 
        }

        public User? CreateUser(string firstName, string lastName, string email, int pin)
        {

            if (string.IsNullOrWhiteSpace(firstName)) return null;

            if (string.IsNullOrWhiteSpace(lastName)) return null;

            if (string.IsNullOrWhiteSpace(email)) return null;

            if (pin.ToString().Length > 6 || pin.ToString().Length < 4) return null;

            if (pin < 0) return null;

            User newUser = new User(firstName, lastName, email, pin);

            using var connection = _dbManager.GetConnection();

            var insertUser = "INSERT INTO Users (FirstName, LastName, UserRole, UserEmail, UserPin) VALUES (@FirstName, @LastName, @UserRole, @UserEmail, @UserPin); " +
                             "SELECT last_insert_rowid();";

            newUser.UserId = connection.ExecuteScalar<int>(insertUser, newUser);

            return newUser;
        }

        public User? GetUserById(int userId) 
        {
            using var connection = _dbManager.GetConnection();

            string getUserQuery = "SELECT UserId, FirstName, LastName, UserRole, UserEmail, UserPin FROM Users WHERE UserId = @UserId";

            var retrievedUser = connection.QueryFirstOrDefault<User>(getUserQuery, new {UserId = userId});

            return retrievedUser;
        }

        public User? GetUserByPin(int pin) 
        {
            using var connection = _dbManager.GetConnection();

            string getUserQuery = "SELECT UserId, FirstName, LastName, UserRole, UserEmail, UserPin FROM Users WHERE UserPin = @UserPin";
            
            var retrievedUser = connection.QueryFirstOrDefault<User>(getUserQuery, new {UserPin = pin});

            return retrievedUser;
        }

        public void DeleteUser(int userId) 
        {
            User? user = GetUserById(userId);

            if (user == null) return;

            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "DELETE FROM Users WHERE UserId = @UserId", new {UserId = userId}
            );
        }

        public User? UpdateUserPin(int userId, int pin)
        {
            var updatedUser = GetUserById(userId);

            if (updatedUser == null) return null;
            if (pin < 0) return null;
            if (pin.ToString().Length > 6 || pin.ToString().Length < 4) return null;


            using var connection = _dbManager.GetConnection();
            
            connection.Execute(
                "UPDATE Users SET UserPin = @UserPin WHERE UserId = @UserId",
                new {UserPin = pin, UserId = userId}
            );

            

            return GetUserById(userId);
        }

        public User? UpdateUserEmail(int userId, string email) 
        {
            var updatedUser = GetUserById(userId);

            if (updatedUser == null) return null;

            if (string.IsNullOrWhiteSpace(email)) return null;

            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "UPDATE Users SET UserEmail = @UserEmail WHERE UserId = @UserId",
                new {UserId = userId, UserEmail = email}
            );



            return GetUserById(userId);
        }

        public User? UpdateUserRole(int userId, string role)
        {
            var updatedItem = GetUserById(userId);
            if (updatedItem == null) return null;

            if (string.IsNullOrWhiteSpace(role)) return null;

            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "UPDATE Users SET UserRole = @UserRole WHERE UserId = @UserId",
                new {UserRole = role, UserId = userId}
            );



            return GetUserById(userId);
        }

        public List<User> LoadUsers()
        {
            using var connection = _dbManager.GetConnection();

            string loadUsersQuery = "SELECT UserId, FirstName, LastName, UserRole, UserEmail, UserPin FROM Users";

            var users = connection.Query<User>(loadUsersQuery).ToList();

            return users;
        }
    }
}
