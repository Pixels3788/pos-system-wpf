using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;


// public interface for the menu service
namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IMenuService
    {
        Task<MenuItem?> CreateMenuItem(string name, decimal price, string category);

        Task DeleteMenuItem(int itemId);

        Task<List<MenuItem>> LoadMenuItems();

        Task<MenuItem?> GetItemById(int itemId);

        Task<MenuItem?> UpdateItemPrice(int itemId, decimal newPrice);

        Task<MenuItem?> UpdateItemName(int itemId, string newName);
    }
}
