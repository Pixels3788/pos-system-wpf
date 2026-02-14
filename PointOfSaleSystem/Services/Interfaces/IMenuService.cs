using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;



namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IMenuService
    {
        MenuItem? CreateMenuItem(string name, decimal price, string category);

        void DeleteMenuItem(int itemId);

        List<MenuItem> LoadMenuItems();

        MenuItem? GetItemById(int itemId);

        MenuItem? UpdateItemPrice(int itemId, decimal newPrice);

        MenuItem? UpdateItemName(int itemId, string newName);
    }
}
