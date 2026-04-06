using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RamsCottons.Components;
using RamsCottons.Components.Account;
using RamsCottons.Data;
using RamsCottons.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICIOS BASE ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- IDENTITY ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- SERVICIO DE EMAIL ---
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

// --- SERVICIO DE QR ---
builder.Services.AddScoped<QrGeneratorService>();

// --- SERVICIO DE PERMISOS ---
builder.Services.AddScoped<PermisoService>();

// --- SERVICIO DE ESTADÍSTICAS (CACHÉ) ---
builder.Services.AddScoped<StatsService>();

// --- AUTORIZACIÓN ---
builder.Services.AddAuthorization();

var app = builder.Build();

// --- CREAR ROLES Y SUPERADMIN ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        // 1. Crear los 4 roles
        string[] roles = { "SuperAdministrador", "Administrador", "Gerente", "Vendedor" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"Rol '{role}' creado.");
            }
        }
        
        // 2. Crear SuperAdministrador por defecto
        string adminEmail = "superadmin@ramscottons.com";
        string adminPassword = "Admin123!";
        
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NombreCompleto = "Super Administrador",
                EmailConfirmed = true,
                Telefono = "9999999999",
                Activo = true
            };
            
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdministrador");
                Console.WriteLine($"SuperAdmin '{adminEmail}' creado.");
            }
            else
            {
                foreach (var error in createResult.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error inicializando: {ex.Message}");
    }
}

// --- PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();