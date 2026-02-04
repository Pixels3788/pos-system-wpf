using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IInventoryMenuCoordinator
    {
        List<InventoryItem> ReconstructInventoryItems();

        void CreateInventoryForMenuItem(string name, decimal price, string category, int startingQuantity);
    }
}
