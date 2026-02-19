using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

// public interface for the user service
namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IUserService
    {

        Task<User?> CreateUser(string firstName, string lastName, string email, int pin);

        Task<User?> GetUserById(int userId);

        Task<User?> GetUserByPin(int pin);

        Task DeleteUser(int userId);

        Task<User?> UpdateUserPin(int userId, int pin);

        Task<User?> UpdateUserEmail(int userId, string email);

        Task<User?> UpdateUserRole(int userId, string role);

        Task<List<User>> LoadUsers();

    }
}
