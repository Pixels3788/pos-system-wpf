using System;
using System.Collections.Generic;
using System.Text;

// public interface for the dialog service
namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IDialogService
    {
        void ShowError(string message, string title);
    }
}
