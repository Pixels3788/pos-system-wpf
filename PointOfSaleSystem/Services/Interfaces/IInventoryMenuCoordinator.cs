using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

// public interface for the inventory menu coordinator
namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IInventoryMenuCoordinator
    {
        Task<List<InventoryItem>> ReconstructInventoryItems();

        void CreateInventoryForMenuItem(string name, decimal price, string category, int startingQuantity);
    }
}
