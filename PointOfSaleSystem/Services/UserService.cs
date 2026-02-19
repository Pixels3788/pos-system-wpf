using PointOfSaleSystem.Database;
using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Models;

using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using PointOfSaleSystem.Database.Interfaces;
using Microsoft.Data.Sqlite;
using Serilog;

// User service that is responsible for CRUD operations for the users table in the database
namespace PointOfSaleSystem.Services
{
    public class UserService : IUserService
    {
        private IDbManager _dbManager;

        public UserService(IDbManager dbManager)
        {
            _dbManager = dbManager; 
        }

        public async Task<User?> CreateUser(string firstName, string lastName, string email, int pin)
        {

            if (string.IsNullOrWhiteSpace(firstName)) return null;

            if (string.IsNullOrWhiteSpace(lastName)) return null;

            if (string.IsNullOrWhiteSpace(email)) return null;

            if (pin.ToString().Length > 6 || pin.ToString().Length < 4) return null;

            if (pin < 0) return null;

            try
            {
                User newUser = new User(firstName, lastName, email, pin);

                using var connection = _dbManager.GetConnection();

                var insertUser = "INSERT INTO Users (FirstName, LastName, UserRole, UserEmail, UserPin) VALUES (@FirstName, @LastName, @UserRole, @UserEmail, @UserPin); " +
                                 "SELECT last_insert_rowid();";

                newUser.UserId = await connection.ExecuteScalarAsync<int>(insertUser, newUser);

                Log.Information("New User Created: Name: {FirstName} {LastName} - User ID: {UserId}", newUser.FirstName, newUser.LastName, newUser.UserId);

                return newUser;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error when creating new user");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected Error creating new user");
                return null;
            }
        }

        public async Task<User?> GetUserById(int userId) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getUserQuery = "SELECT UserId, FirstName, LastName, UserRole, UserEmail, UserPin FROM Users WHERE UserId = @UserId";

                var retrievedUser = await connection.QueryFirstOrDefaultAsync<User>(getUserQuery, new { UserId = userId });

                if (retrievedUser != null)
                {
                    Log.Information("User Retrieved From Database: Name: {FirstName} {LastName} - User ID: {UserId}", retrievedUser.FirstName, retrievedUser.LastName, retrievedUser.UserId);
                }
                else
                {
                    Log.Warning("User {UserId} not found", userId);
                }
                return retrievedUser;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error when fetching user with the User ID {UserId}", userId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error when fetching user with the User ID {UserId}", userId);
                return null;
            }
        }

        public async Task<User?> GetUserByPin(int pin) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getUserQuery = "SELECT UserId, FirstName, LastName, UserRole, UserEmail, UserPin FROM Users WHERE UserPin = @UserPin";

                var retrievedUser = await connection.QueryFirstOrDefaultAsync<User>(getUserQuery, new { UserPin = pin });

                if (retrievedUser != null)
                {
                    Log.Information("Successfully fetched the user with the ID {UserId} using their pin number", retrievedUser.UserId);
                }
                else
                {
                    Log.Warning("Authentication failed: Invalid PIN");
                }
                return retrievedUser;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error when fetching user by pin number");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while fetching user by pin number");
                return null;
            }
        }

        public async Task DeleteUser(int userId) 
        {
            try
            {
                User? user = await GetUserById(userId);

                if (user == null)
                {
                    Log.Warning("User Deletion Failed: User ID {UserId} does not exist", userId);
                    return;
                }

                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId = @UserId", new { UserId = userId }
                );
                Log.Information("Successfully Deleted the user with the User ID {UserId}", userId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error when trying to delete user with the User ID {UserId}", userId);
                return;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unexpected error when trying to delete user with the User ID {UserId}", userId);
                return;
            }

        }

        public async Task<User?> UpdateUserPin(int userId, int pin)
        {
            try
            {
                var updatedUser = await GetUserById(userId);

                if (updatedUser == null)
                {
                    Log.Warning("Updating User Pin Failed: User with the User ID {UserId} could not be found", userId);
                    return null;
                }
                if (pin < 0)
                {
                    Log.Warning("Updating User Pin Failed: Invalid pin number was used for the update");
                    return null;
                }
                if (pin.ToString().Length > 6 || pin.ToString().Length < 4)
                {
                    Log.Warning("Updating User Pin Failed: New pin numbers length was invalid");
                    return null;
                }

                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE Users SET UserPin = @UserPin WHERE UserId = @UserId",
                    new { UserPin = pin, UserId = userId }
                );

                Log.Information("Successfully updated the pin number for the user with the User ID {UserId}", userId);

                return await GetUserById(userId);
            }
            catch(SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the pin number of the user with the User ID {UserId}", userId);
                return null;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to update the pin number of the user with User ID {UserId}", userId);
                return null;
            }

            

            
        }

        public async Task<User?> UpdateUserEmail(int userId, string email) 
        {
            try
            {
                var updatedUser = await GetUserById(userId);

                if (updatedUser == null)
                {
                    Log.Warning("User Email Update Failed: Could not find a user with the User ID {UserId}", userId);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    Log.Warning("User Email Update Failed: updated email was invalid");
                    return null;
                }

                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE Users SET UserEmail = @UserEmail WHERE UserId = @UserId",
                    new { UserId = userId, UserEmail = email }
                );

                return await GetUserById(userId);
            }
            catch(SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the email of the user with the User ID {UserId}", userId);
                return null;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to update the email of the user with the User ID {UserId}", userId);
                return null;
            }
        }

        public async Task<User?> UpdateUserRole(int userId, string role)
        {
            try
            {
                var updatedItem = await GetUserById(userId);
                if (updatedItem == null) return null;

                if (string.IsNullOrWhiteSpace(role)) return null;

                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "UPDATE Users SET UserRole = @UserRole WHERE UserId = @UserId",
                    new { UserRole = role, UserId = userId }
                );

                return await GetUserById(userId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to update the role of the user with the User ID {UserId}", userId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to update the role of the user with the User ID {UserId}", userId);
                return null;
            }
        }

        public async Task<List<User>> LoadUsers()
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string loadUsersQuery = "SELECT UserId, FirstName, LastName, UserRole, UserEmail, UserPin FROM Users";

                var users = await connection.QueryAsync<User>(loadUsersQuery);

                var usersList = users.ToList();

                Log.Information("Successfully retrieved {Count} users from the database", usersList.Count);

                return usersList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while attempting to retrieve the users from the users table");
                return new List<User>();
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to retrieve the users from the users table");
                return new List<User>();
            }
        }
    }
}
