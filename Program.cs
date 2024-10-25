using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Areas.Identity.Data;
using AnyoneForTennis.Helpers;
using AnyoneForTennis.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure the database connection
var connectionString = builder.Configuration.GetConnectionString("AnyoneForTennisContextConnection")
    ?? throw new InvalidOperationException("Connection string 'AnyoneForTennisContextConnection' not found.");

builder.Services.AddDbContext<AnyoneForTennisContext>(options =>
    options.UseSqlite(connectionString));

// Configure Identity services
builder.Services.AddDefaultIdentity<AnyoneForTennisUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AnyoneForTennisContext>();

// Add additional services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<EmailHelper>();
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize roles and admin account on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await InitializeRolesAndAdminAsync(services);
}

app.Run();

// Helper method to initialize roles and admin user
static async Task InitializeRolesAndAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AnyoneForTennisUser>>();

    var roles = new[] { "Admin", "Member", "Coach" };

    // Create roles if they don't exist
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to create role: {role}");
            }
        }
    }

    // Create and assign Admin user
    var adminEmail = "anyonefortennis_admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new AnyoneForTennisUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createUserResult = await userManager.CreateAsync(adminUser, "Password@2024");
        if (!createUserResult.Succeeded)
        {
            var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create admin user: {errors}");
        }
    }

    // Assign Admin role to the admin user if not already assigned
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!addToRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to assign 'Admin' role: {errors}");
        }
    }
}
