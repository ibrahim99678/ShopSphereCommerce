# ShopSphere | An online marketplace

ASP.NET Core MVC + EF Core (SQL Server) e-commerce application with admin panel (sidebar UI), customer accounts, orders/payments, reviews, inventory, and notifications.

## Tech Stack
- .NET 8 (ASP.NET Core MVC + Razor Pages Identity UI)
- Entity Framework Core 8 + SQL Server
- Bootstrap (UI)

## Features (Current)
**Customer**
- Authentication (register/login/logout)
- Profile management (profile image upload, addresses)
- Browse products (category/brand filters, sorting, pagination)
- Product details (images gallery, variants, reviews & rating summary)
- Shopping cart (session + user carts)
- Checkout (stock validation, optional delivery charge)
- Orders & order details
- Payment flow (mock provider)
- Notifications (bell + unread badge, in-app list)

**Admin**
- Admin dashboard (total orders, paid sales, customers, recent orders)
- Admin sidebar panel layout
- Product management
- Category management
- Product variants (size/color, price override, stock)
- Product image management (primary image support)
- Order management (status transitions)
- Review moderation (approve/delete)
- Inventory low stock page (threshold filter)
- Admin notifications (new order alerts)
- User & role management (Admin/Customer roles)

## Projects
- `ShopSphere.Web` – MVC web application + Identity UI + Admin area
- `ShopSphere.BLL` – business logic/services
- `ShopSphere.DAL` – EF Core DbContext, repositories, migrations, UnitOfWork
- `ShopSphere.Domain` – entities/enums
- `ShopSphere.Contract` – DTOs and service contracts

## Getting Started

### 1) Prerequisites
- .NET SDK 8.x
- SQL Server (or LocalDB)

### 2) Configure the database connection
Update the connection string in:
- `ShopSphere.Web/appsettings.json`

Key:
- `ConnectionStrings:DefaultConnection`

### 3) Run the application
From the solution folder:

```bash
dotnet run --project .\ShopSphere.Web\ShopSphere.Web.csproj
```

The default development profile uses:
- https://localhost:7035
- http://localhost:5183

### 4) Database migrations
The application applies migrations on startup.

If you want to apply them manually:

```bash
dotnet ef database update -p .\ShopSphere.DAL\ShopSphere.DAL.csproj -s .\ShopSphere.Web\ShopSphere.Web.csproj --context ShopSphereDbContext
```

## Admin Account (Dev Seed)
On startup, roles are created and a default admin user is seeded in Development if not provided.

Defaults (Development only):
- Email: `admin@shopsphere.com`
- Password: `Admin@123`

Override via configuration:
- `Seed:AdminEmail`
- `Seed:AdminPassword`

Seeder: `ShopSphere.Web/Identity/IdentitySeeder.cs`

## Email + Notifications

### Email sender behavior
- If `Smtp:Host` is not configured, a development email sender logs emails to the console.
- If `Smtp:Host` is configured, SMTP is used.

SMTP config keys:
- `Smtp:Host`
- `Smtp:Port` (default 587)
- `Smtp:User`
- `Smtp:Pass`
- `Smtp:From` (optional; falls back to `Smtp:User`)
- `Smtp:EnableSsl` (set to `false` to disable)

### Notification events
- Order placed → customer notification + email (if customer is registered)
- Order placed → admin notification + email
- Order shipped → customer notification + email

In-app notifications are stored in the database and shown via the bell icon with unread badge.

## Email confirmation (Dev vs Prod)
- Development: email confirmation is not required to sign in.
- Production: email confirmation is required to sign in.

Controlled in `ShopSphere.Web/Program.cs`:
- `options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment();`

## Payments
Payments use a mock provider for now:
- Provider: `MockPay`

Payment controller: `ShopSphere.Web/Controllers/PaymentController.cs`

## Delivery charge (Optional)
Checkout can optionally add a delivery charge.
- Stored on the order as `ShippingCharge` (nullable)
- Included in the order `Total`

## Notes
- Stock is validated in cart updates and checkout/order creation.
- Admin area uses a dedicated layout with sidebar navigation.
