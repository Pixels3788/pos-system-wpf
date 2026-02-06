using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Services;
using System.Configuration;
using System.Data;
using System.Windows;
using PointOfSaleSystem.ViewModels;


namespace PointOfSaleSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnMainWindowClose;

            var navigationService = new NavigationService();

            var mainWindowVM = new MainWindowViewModel(navigationService);
            var mainWindow = new MainWindow(mainWindowVM);

            MainWindow = mainWindow;
            mainWindow.Show();
        }

    }

}
