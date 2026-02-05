using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Database.Interfaces
{
    public interface IDbManager : IDisposable
    {
        SqliteConnection GetConnection();
    }
}
