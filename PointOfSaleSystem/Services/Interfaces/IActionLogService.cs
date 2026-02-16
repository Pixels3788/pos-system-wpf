using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IActionLogService
    {

        Task<ActionLog?> CreateActionLog(User user, string action, string description);

        Task DeleteActionLog(int logId);

        Task<List<ActionLog>> GetActionLogs();

        Task<ActionLog?> GetLogById(int logId);



    }
}
