using Microsoft.EntityFrameworkCore;
using SchoolQueueSystem.Data;

// ════════════════════════════════════════════════════════════════
//  PROGRAM.CS  –  The entry point / startup configuration
// ════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ── 1. Register Services ─────────────────────────────────────────

// SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=queue.db"));

// API controllers
builder.Services.AddControllers();

// Session support — this lets us remember who is logged in between requests.
// Sessions are stored in server memory and tied to a cookie in the browser.
builder.Services.AddDistributedMemoryCache(); // In-memory store for sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);  // Session lasts 8 hours
    options.Cookie.HttpOnly = true;               // JS cannot read the cookie (security)
    options.Cookie.IsEssential = true;            // Required for the app to work
});

// ── 2. Build the App ─────────────────────────────────────────────
var app = builder.Build();

// ── 3. Auto-create / Migrate the Database ────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ── 4. Middleware Pipeline ────────────────────────────────────────

// Serve static files (HTML, CSS, JS) from wwwroot/
app.UseStaticFiles();

// IMPORTANT: UseSession() must come BEFORE MapControllers()
// This makes the session available in all controllers
app.UseSession();

// ── 5. Route Guard for Admin Pages ───────────────────────────────
// This middleware runs before every request.
// If someone tries to access /admin/index.html without being logged in,
// we redirect them to the login page instead.
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";

    // Check if they are trying to reach the admin dashboard (not the login/register pages)
    bool isAdminDashboard = path.StartsWith("/admin/index.html", StringComparison.OrdinalIgnoreCase);

    if (isAdminDashboard)
    {
        // Check if they have a valid session
        var username = context.Session.GetString("AdminUsername");
        if (string.IsNullOrEmpty(username))
        {
            // Not logged in -> send them to the login page
            context.Response.Redirect("/admin/login.html");
            return;
        }
    }

    await next(); // Everything is fine, continue normally
});

// Enable routing to controllers
app.MapControllers();

// Default route: redirect / to the student display board
app.MapGet("/", () => Results.Redirect("/student/index.html"));

// ── 6. Start the Server ──────────────────────────────────────────
Console.WriteLine("=================================================");
Console.WriteLine("  School Queue System is running!");
Console.WriteLine("  Student Display : http://localhost:5000/student/index.html");
Console.WriteLine("  Admin Login     : http://localhost:5000/admin/login.html");
Console.WriteLine("  Admin Panel     : http://localhost:5000/admin/index.html");
Console.WriteLine("  API Base URL    : http://localhost:5000/api/queue");
Console.WriteLine("=================================================");

app.Run("http://localhost:5000");
