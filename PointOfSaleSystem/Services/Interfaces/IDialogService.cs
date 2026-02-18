using System;
using System.Collections.Generic;
using System.Text;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface IDialogService
    {
        void ShowError(string message, string title);
    }
}
