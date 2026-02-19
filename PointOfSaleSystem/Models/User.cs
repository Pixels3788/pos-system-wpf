using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

// data class that represents a user
namespace PointOfSaleSystem.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string FirstName { get;  set; }

        public string LastName { get;  set; }

        public string UserRole { get; set; }

        public int UserPin {  get;  set; }

        public string UserEmail { get; set; }

        public User(string firstName, string lastName, string userEmail, int userPin)
        {
            FirstName = firstName;
            LastName = lastName;
            UserEmail = userEmail;
            UserPin = userPin;
            UserRole = "Associate";
        }
        public User() { }

        public void ChangeUserEmail(string newEmail)
        {
            UserEmail = newEmail; 
        }

        public void ResetUserPin() 
        {
            UserPin = Random.Shared.Next(1000, 999999);
        }
    }
}
