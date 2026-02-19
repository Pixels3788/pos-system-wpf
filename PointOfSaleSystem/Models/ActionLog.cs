using System;
using System.Collections.Generic;
using System.Text;


// Data model for the action logs managers can view in the main program
namespace PointOfSaleSystem.Models
{
    public class ActionLog
    {

        public string Action {  get; set; }

        public string Description { get; private set; }

        public DateTime Timestamp { get; set; } 

        public int UserId{ get; set; }

        public int LogId { get; set; }

        public ActionLog(int userId, string action, string description)
        {
            Action = action;
            Description = description;
            UserId = userId;
            
        }
        public ActionLog() { }



    }
}
