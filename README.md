# Point of Sale System
A full-featured Point of Sale desktop application built with WPF and C#, following the MVVM architectural pattern. Designed to simulate a real-world restaurant or fast food POS environment, with role-based access control, inventory management, order tracking, and detailed operational logging.

---

## Demo

[![Point of Sale System Demo](https://img.youtube.com/vi/dw89h7kHpHU/maxresdefault.jpg)](https://www.youtube.com/watch?v=dw89h7kHpHU)

*Click the thumbnail to watch the full demo on YouTube.*

---

## Features

- **Role-Based Access Control** — Associates can take and manage orders. Managers have access to an expanded panel for editing the menu, managing inventory, managing users, and viewing action logs.
- **Order Management** — Create, modify, finalize, and cancel orders with real-time inventory tracking and automatic availability updates.
- **Menu Editor** — Managers can add, edit, and remove menu items with linked inventory entries.
- **Inventory Editor** — Track and update stock quantities with automatic availability toggling based on inventory levels.
- **User Editor** — Managers can create, edit, and remove user accounts and assign roles.
- **Action Logs** — A detailed log of all significant user actions across the application, viewable from the manager panel.
- **Structured Logging** — Serilog captures detailed technical logs at every layer of the application, written to rolling daily log files.
- **Error Handling** — User-facing error dialogs for critical failures, backed by structured Serilog error logs for debugging.

---

## Architecture

This project strictly follows the **MVVM (Model-View-ViewModel)** pattern with a clear separation of concerns across four layers:

- **Models** — Plain data classes representing the core entities: `User`, `Order`, `OrderLineItem`, `MenuItem`, `InventoryItem`, `ActionLog`.
- **Services** — The business logic layer. Each service is defined by an interface and handles all database interaction for its domain. Services include `OrderService`, `MenuService`, `InventoryService`, `UserService`, `ActionLogService`, `NavigationService`, and `DialogService`, as well as two coordinator services (`OrderInventoryCoordination`, `InventoryMenuCoordinator`) that manage cross-domain operations.
- **ViewModels** — One ViewModel per screen. All ViewModels inherit from `BaseViewModel` which implements `INotifyPropertyChanged`. Commands are implemented via custom `RelayCommand`, `AsyncRelayCommand`, and their generic variants.
- **Views** — Pure XAML with empty code-behind files. All logic lives in the ViewModels.

### Navigation
Navigation is handled by a custom `NavigationService` that uses a factory registry pattern. ViewModels are registered at startup with their construction factories, and navigation is triggered by type. ViewModels that require parameters implement the `INavigable` interface and receive data through an `Initialize(object parameter)` method.

### Command Handling
A custom `AsyncRelayCommand` (and generic `AsyncRelayCommand<T>`) wraps `Func<Task>` to safely bridge the gap between WPF's synchronous `ICommand.Execute` and async ViewModel methods. All async command errors are caught at the command level and logged via Serilog, ensuring unhandled exceptions never crash the application.

### Dependency Injection
All services and dependencies are manually composed in `App.xaml.cs` at startup and injected through constructors, maintaining loose coupling and testability throughout the application.

---

## Tech Stack

| Technology | Purpose |
|---|---|
| C# / .NET | Core language and runtime |
| WPF | UI framework |
| SQLite | Local relational database |
| Dapper | Lightweight ORM for database queries |
| Serilog | Structured logging to file and console |
| xUnit | Integration testing for the service layer |
| FluentAssertions | Test assertions |

---

## Screens

| Screen | Access |
|---|---|
| Login | All users |
| Create New User | All users (from login) |
| Order Taking | All users |
| Open Orders | All users |
| Finalized Orders | All users |
| Manager Panel | Managers only |
| Menu Editor | Managers only |
| Inventory Editor | Managers only |
| User Editor | Managers only |
| Action Logs | Managers only |

---

## Testing

The project includes a dedicated integration test suite covering the core service layer. Tests run against an in-memory SQLite database for full isolation, ensuring no shared state between test runs.

**Test coverage includes:**

| Service | Tests |
|---|---|
| `OrderService` | Order creation, deletion, finalization, line item CRUD, quantity updates, open/finalized order retrieval |
| `UserService` | User creation, deletion, retrieval by ID and PIN, role/email/PIN updates, input validation |
| `InventoryService` | Inventory creation, deletion, retrieval, increment/decrement operations, validation |
| `MenuService` | Menu item creation, deletion, retrieval, price updates, input validation |

Both happy path and failure/validation cases are covered throughout. Parameterized tests (`[Theory]` with `[InlineData]`) are used extensively to verify boundary conditions and invalid input handling across multiple scenarios without duplicating test logic.

### Running Tests
```
dotnet test
```
Or via Visual Studio Test Explorer.

---

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022 

### Running the Application
1. Clone the repository
2. Open `PointOfSaleSystem.slnx` in Visual Studio
3. Build and run the project

The database will be automatically created at `Database/pos_system.db` on first run. A seed button is available on the order taking screen to populate the menu and create a test manager account if needed.

---

## Project Structure

```
PointOfSaleSystem/
├── Models/                  # Core data models
├── ViewModels/              # One ViewModel per screen + BaseViewModel
├── Views/                   # XAML views with empty code-behind
├── Services/
│   ├── Interfaces/          # Service contracts
│   └── *.cs                 # Service implementations
├── Database/
│   ├── Interfaces/
│   └── DbManager.cs         # Database initialization and connection management
├── Helpers/
│   └── RelayCommand.cs      # RelayCommand, AsyncRelayCommand, and generic variants
└── App.xaml.cs              # Application startup and dependency composition

PointOfSaleTests/
├── OrderServiceTests.cs
├── UserServiceTests.cs
├── InventoryServiceTests.cs
└── MenuServiceTests.cs
```

---

## What I Learned
This project taught me a great deal about how to develop software, creating the database schema, the service layer to query the database, and the view models to communicate the data from the service layers to the UI gave me a much better grasp on how important architecture is when it comes to developing a project. It also taught me why it is so important to maintain a clean code base with a good separation of concerns, that is self documenting and easy to navigate. A project can become a nightmare to add on to, refactor, or generally change anything about if you don't make a serious effort to keep things tidy. 

This project also taught me what dependency injection is and why it is so useful, which is a concept that I am sure will greatly benefit me in my future as a developer. I also gained a lot of valuable problem solving skills because I constantly had to think about how to solve certain issues I had run into or think about how to implement certain features that I wanted. And even though I already knew that the ability to research and find answers as a developer was important, I got to really see how important it is because I constantly had to do deep dives into documentation to find information or a solution to an issue or question I had. 

I also learned a lot about the value of testing your code and writing integration tests and test cases. Outside of school, this is the first time I have written them for a personal project of mine and doing it not only made me better at writing them, but taught me their value. They allowed me to make various changes to things and then run the tests to ensure those changes did not create new bugs. That was invaluable while refactoring things over time. I also got to see the value in using something like serilog to maintain detailed logs throughout the program for debugging purposes and identifying where errors are happening. Both of these things are skills I know I will carry over to future projects.

---



