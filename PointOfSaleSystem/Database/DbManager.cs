using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Data.Sqlite;
using System.DirectoryServices;
using System.Dynamic;
using PointOfSaleSystem.Database.Interfaces;

namespace PointOfSaleSystem.Database
{
    public class DbManager : IDbManager
    {
        private const string DefaultDbPath = "Database/pos_system.db";
        private readonly string _connectionString;

        private readonly bool _isInMemory;

        private SqliteConnection? _keepAliveConnection;


        public DbManager()
        {
            Directory.CreateDirectory("Database");

            _connectionString = $"Data Source={DefaultDbPath}";

            _isInMemory = false;

            InitializeDatabase();
        }
        public DbManager(string connectionString)
        {
            _connectionString = connectionString;
            _isInMemory = connectionString.Contains("Mode=Memory");

            if (_isInMemory) 
            {
                _keepAliveConnection = new SqliteConnection(connectionString);
                _keepAliveConnection.Open();
                using var pragma = _keepAliveConnection.CreateCommand();
                pragma.CommandText = "PRAGMA foreign_keys = ON";
                pragma.ExecuteNonQuery();
            }

            InitializeDatabase();
        }

        public SqliteConnection GetConnection()
        {


            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand(); 
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            return connection;
        }

        private void InitializeDatabase()
        {
            using var connection = GetConnection();

            using var command = connection.CreateCommand();


            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS MenuItems (
                    ItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL,
                    Category TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    UserRole TEXT NOT NULL,
                    UserEmail TEXT NOT NULL,
                    UserPin INTEGER NOT NULL
                );";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CreatedAt TEXT NOT NULL,
                    FinalizedAt TEXT,
                    IsFinalized INTEGER NOT NULL DEFAULT 0
                );";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS OrderLineItems (
                    LineItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    MenuItemId INTEGER NOT NULL,
                    NameAtSale TEXT NOT NULL,
                    UnitPrice REAL NOT NULL,
                    Quantity INTEGER NOT NULL,
                    FOREIGN KEY(OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                    FOREIGN KEY(MenuItemId) REFERENCES MenuItems(ItemId) 
                );";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS InventoryItems (
                    InventoryItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    QuantityOnHand INTEGER NOT NULL,
                    MenuItemId INTEGER NOT NULL,
                    FOREIGN KEY(MenuItemId) REFERENCES MenuItems(ItemId)
                );";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ActionLogs (
                    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Action TEXT NOT NULL,
                    UserId INTEGER NOT NULL,
                    Description TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    FOREIGN KEY(UserId) REFERENCES Users(UserId)
                );";
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _keepAliveConnection?.Dispose();
        }

    }
}
