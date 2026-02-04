using PointOfSaleSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IActionLogService
    {

        ActionLog? CreateActionLog(User user, string action, string description);

        void DeleteActionLog(int logId);

        List<ActionLog> GetActionLogs();

        ActionLog? GetLogById(int logId);



    }
}
