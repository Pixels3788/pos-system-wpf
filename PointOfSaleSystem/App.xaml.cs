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
        }

    }

}
