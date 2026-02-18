using PointOfSaleSystem.Services.Interfaces;
using PointOfSaleSystem.Services;
using System.Configuration;
using System.Data;
using System.Windows;
using PointOfSaleSystem.ViewModels;
using PointOfSaleSystem.Database;
using Serilog;


namespace PointOfSaleSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Debug()
               .WriteTo.Console()
               .WriteTo.File(
                   path: "logs/pos-.txt",
                   rollingInterval: RollingInterval.Day,
                   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
               )
               .CreateLogger();

            Log.Information("=== Application Starting ===");

            try
            {
                base.OnStartup(e);

                ShutdownMode = ShutdownMode.OnMainWindowClose;

                var dbManager = new DbManager();

                INavigationService navigationService = new NavigationService();
                IActionLogService actionLogService = new ActionLogService(dbManager);
                IUserService userService = new UserService(dbManager);
                IInventoryService inventoryService = new InventoryService(dbManager);
                IOrderService orderService = new OrderService(dbManager);
                IMenuService menuService = new MenuService(dbManager);
                IOrderInventoryCoordination inventoryOrderCoordination = new OrderInventoryCoordination(orderService, inventoryService);
                InventoryMenuCoordinator inventoryMenuCoordinator = new InventoryMenuCoordinator(inventoryService, menuService);


                navigationService.Register(() => new LoginScreenViewModel(navigationService, userService, actionLogService));
                navigationService.Register(() => new CreateNewUserViewModel(navigationService, userService, actionLogService));
                navigationService.Register(() => new OrderTakingScreenViewModel(
                    userService,
                    navigationService,
                    inventoryOrderCoordination,
                    orderService,
                    menuService,
                    inventoryMenuCoordinator,
                    inventoryService,
                    actionLogService));
                navigationService.Register(() => new OpenOrdersScreenViewModel(navigationService, orderService, inventoryOrderCoordination, actionLogService));
                navigationService.Register(() => new ClosedOrdersScreenViewModel(navigationService, orderService));
                navigationService.Register(() => new InventoryEditorScreenViewModel(inventoryMenuCoordinator, navigationService, inventoryService, actionLogService));
                navigationService.Register(() => new ManagerPanelScreenViewModel(navigationService));
                navigationService.Register(() => new EditMenuScreenViewModel(navigationService, menuService, inventoryMenuCoordinator, actionLogService, inventoryService));
                navigationService.Register(() => new UserEditorScreenViewModel(navigationService, userService, actionLogService));
                navigationService.Register(() => new LogsScreenViewModel(actionLogService, navigationService));


                navigationService.Navigate<LoginScreenViewModel>();


                var mainWindowVM = new MainWindowViewModel(navigationService);
                var mainWindow = new MainWindow(mainWindowVM);



                MainWindow = mainWindow;
                mainWindow.Show();

                Log.Information("Application started successfully");
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                MessageBox.Show(
                    $"Critical error starting application:\n{ex.Message}\n\nCheck logs folder for details.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== Application Shutting Down ===");
            Log.CloseAndFlush();
            base.OnExit(e);
        }

    }

}
