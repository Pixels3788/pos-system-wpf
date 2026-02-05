using Dapper;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using PointOfSaleSystem.Database.Interfaces;

namespace PointOfSaleSystem.Services
{
    internal class ActionLogService : IActionLogService
    {

        private IDbManager _dbManager;

        public ActionLogService(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public ActionLog? CreateActionLog(User user, string action, string description)
        {

            if (user == null) return null;

            if (string.IsNullOrWhiteSpace(action)) return null;
            
            ActionLog newLog = new ActionLog(user.UserId, action, description);
            newLog.Timestamp = DateTime.Now;

            using var connection = _dbManager.GetConnection();

            string addNewLog = "INSERT INTO ActionLogs (Action, UserId, Description, Timestamp) VALUES (@Action, @UserId, @Description, @Timestamp);" +
                               "SELECT last_insert_rowid();" ;

            newLog.LogId = connection.ExecuteScalar<int>(addNewLog, newLog);

            return newLog;

        }

        public void DeleteActionLog(int logId)
        {
            if (logId <= 0) return;
            using var connection = _dbManager.GetConnection();

            connection.Execute(
                "DELETE FROM ActionLogs WHERE LogId = @LogId", new {LogId = logId}
            );
        }

        public List<ActionLog> GetActionLogs()
        {
            using var connection = _dbManager.GetConnection();

            string getActionLogs = "SELECT LogId, Action, UserId, Description, Timestamp FROM ActionLogs";

            var retrievedLogs = connection.Query<ActionLog>(getActionLogs).ToList();

            return retrievedLogs;
        }

        public ActionLog? GetLogById(int logId) 
        {
            using var connection = _dbManager.GetConnection();

            string getLogByIdQuery = "SELECT LogId, Action, UserId, Description, Timestamp FROM ActionLogs WHERE LogId = @LogId";

            var retrievedLog = connection.QueryFirstOrDefault<ActionLog>(getLogByIdQuery, new { LogId = logId });

            return retrievedLog;
        }

    }
}
