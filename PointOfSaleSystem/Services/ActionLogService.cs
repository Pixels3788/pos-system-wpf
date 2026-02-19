using Dapper;
using PointOfSaleSystem.Database;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using PointOfSaleSystem.Database.Interfaces;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Serilog;


// Action log service that is responsible for CRUD operations for the actions logs table of the db
namespace PointOfSaleSystem.Services
{
    internal class ActionLogService : IActionLogService
    {

        private IDbManager _dbManager;

        public ActionLogService(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<ActionLog?> CreateActionLog(User user, string action, string description)
        {

            if (user == null)
            {
                Log.Warning("CreateActionLog Failed: User is null");
                return null;
            }
            if (string.IsNullOrWhiteSpace(action))
            {
                Log.Warning("CreateActionLog failed: Action is empty for user {UserId}", user?.UserId);
                return null;
            }


            try
            {
                ActionLog newLog = new ActionLog(user.UserId, action, description);
                newLog.Timestamp = DateTime.Now;

                using var connection = _dbManager.GetConnection();

                string addNewLog = "INSERT INTO ActionLogs (Action, UserId, Description, Timestamp) VALUES (@Action, @UserId, @Description, @Timestamp);" +
                                   "SELECT last_insert_rowid();";

                newLog.LogId = await connection.ExecuteScalarAsync<int>(addNewLog, newLog);

                Log.Information("Action log created: {LogId} - User {UserId} - {Action}", newLog.LogId, user.UserId, action);

                return newLog;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected error creating action log for user {UserId}", user.UserId);

                return null;
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Unexpected Error creating action log for user {UserId}", user.UserId);
                return null;
            }

        }

        public async Task DeleteActionLog(int logId)
        {
            if (logId <= 0)
            {
                Log.Warning("Log Deletion Failed: Log ID {LogId} does not exist", logId);
                return;
            }
            try
            {
                using var connection = _dbManager.GetConnection();

                await connection.ExecuteAsync(
                    "DELETE FROM ActionLogs WHERE LogId = @LogId", new { LogId = logId }
                );

                Log.Information("Action log deleted: {LogId}", logId);
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error deleting action log {LogId}", logId);
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error deleting action log {LogId}", logId);
                return;
            }
        }

        public async Task<List<ActionLog>> GetActionLogs()
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getActionLogs = "SELECT LogId, Action, UserId, Description, Timestamp FROM ActionLogs";

                var retrievedLogs = await connection.QueryAsync<ActionLog>(getActionLogs);

                var logList = retrievedLogs.ToList();

                Log.Information("Retrieved {Count} action logs", logList.Count);

                return logList;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected Database error fetching action logs");
                return new List<ActionLog>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error fetching action logs");
                return new List<ActionLog>();
            }
        }

        public async Task<ActionLog?> GetLogById(int logId) 
        {
            try
            {
                using var connection = _dbManager.GetConnection();

                string getLogByIdQuery = "SELECT LogId, Action, UserId, Description, Timestamp FROM ActionLogs WHERE LogId = @LogId";

                var retrievedLog = await connection.QueryFirstOrDefaultAsync<ActionLog>(getLogByIdQuery, new { LogId = logId });
                if (retrievedLog != null)
                {
                    Log.Information("successfully retrieved action log {LogId} from the database", logId);
                }
                else
                {
                    Log.Warning("Action log {LogId} not found", logId);
                }
                return retrievedLog;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Unexpected database error while trying to retrieve action log {LogId}", logId);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error retrieving action log {LogId}", logId);
                return null;
            }
        }

    }
}
