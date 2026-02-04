using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IUserService
    {

        User? CreateUser(string firstName, string lastName, string email, int pin);

        User? GetUserById(int userId);

        User? GetUserByPin(int pin);

        void DeleteUser(int userId);

        User? UpdateUserPin(int userId, int pin);

        User? UpdateUserEmail(int userId, string email);

        User? UpdateUserRole(int userId, string role);

        List<User> LoadUsers();

    }
}
