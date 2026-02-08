using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Services;
using System.Configuration;
using System.Data;
using System.Windows;
using PointOfSaleSystem.ViewModels;
using PointOfSaleSystem.Database;


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

            var dbManager = new DbManager();

            var navigationService = new NavigationService();
            var userService = new UserService(dbManager);

            var mainWindowVM = new MainWindowViewModel(navigationService);
            var mainWindow = new MainWindow(mainWindowVM);
            var loginScreenVM = new LoginScreenViewModel(navigationService, userService);
            navigationService.CurrentViewModel = loginScreenVM;

            

            MainWindow = mainWindow;
            mainWindow.Show();
        }

    }

}
