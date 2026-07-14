using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Configure DbContext with SQLite
// Use different paths for development and production
string dbPath;
if (builder.Environment.IsDevelopment())
{
    // Use the local database file you just created
    dbPath = "local.db";
    Console.WriteLine("🌐 Running in Development mode");
}
else
{
    // Production/Render path
    dbPath = "/app/data/grandarbre.db";
    Console.WriteLine("🚀 Running in Production mode");

    // Ensure directory exists in production
    var directory = Path.GetDirectoryName(dbPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

var connectionString = $"Data Source={dbPath}";
Console.WriteLine($"📊 Using database: {dbPath}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/UserLogin";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        // In production, apply migrations. In development, use EnsureCreated
        if (app.Environment.IsProduction())
        {
            Console.WriteLine("🔄 Applying migrations...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Migrations applied");
        }
        else
        {
            // Development - database already exists from Update-Database
            Console.WriteLine("✅ Using existing database");
        }

        // Seed roles
        string[] roles = { "Admin", "Employee", "Client" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✅ Created role: {role}");
            }
        }

        // Create Admin user
        var adminEmail = "admin@grandarbre.co.za";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                CreatedAt = DateTime.Now,
                IsActive = true,
                AccessCode = "ADMIN2024",
                IsFirstLogin = false
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Admin user created");
            }
        }

        Console.WriteLine("🎉 Database initialization completed!");
        Console.WriteLine("========================================");
        Console.WriteLine("📝 Test Credentials:");
        Console.WriteLine("   Admin: admin@grandarbre.co.za / Admin123!");
        Console.WriteLine("========================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();