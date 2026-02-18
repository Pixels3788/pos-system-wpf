using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace PointOfSaleSystem.Services
{
    public class DialogService : IDialogService
    {

        public DialogService() { }

        public void ShowError(string message, string title) 
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowConfirmation(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo);

            return result == MessageBoxResult.Yes;
        }

    }
}
