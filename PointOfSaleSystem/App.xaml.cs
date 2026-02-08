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

            navigationService.Register(() => new LoginScreenViewModel(navigationService, userService));
            navigationService.Register(() => new CreateNewUserViewModel(navigationService, userService));

            navigationService.Navigate<LoginScreenViewModel>();

            var mainWindowVM = new MainWindowViewModel(navigationService);
            var mainWindow = new MainWindow(mainWindowVM);

            

            MainWindow = mainWindow;
            mainWindow.Show();
        }

    }

}
