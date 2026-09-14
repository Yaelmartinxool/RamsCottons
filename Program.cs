using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features;
using RamsCottons.Components;
using RamsCottons.Components.Account;
using RamsCottons.Data;
using RamsCottons.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICIOS BASE --- //
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

// ==========================================================
// ✅ CONFIGURACIÓN PARA ARCHIVOS GRANDES (hasta 100MB)
// ==========================================================
builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 100 * 1024 * 1024; // 100 MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// --- BASE DE DATOS --- //
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
    }));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- CONTEXTO PARA CYDATA (SOLO LECTURA) --- //
var cycDataConnectionString = builder.Configuration.GetConnectionString("CycDataConnection")
    ?? throw new InvalidOperationException("Connection string 'CycDataConnection' not found.");

builder.Services.AddDbContext<CycDataContext>(options =>
    options.UseSqlServer(cycDataConnectionString, sqlOptions =>
    {
        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_CycData");
    }));

// --- IDENTITY --- //
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

// --- SERVICIOS DE APLICACION --- //
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddScoped<QrGeneratorService>();
builder.Services.AddScoped<PermisoService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<SucursalService>();
builder.Services.AddScoped<UserSucursalService>();

// --- SERVICIO DE WHATSAPP --- //
builder.Services.AddHttpClient();
builder.Services.AddScoped<WhatsAppService>();

// --- AUTORIZACION --- //
builder.Services.AddAuthorization();

var app = builder.Build();

// --- CREAR ROLES Y SUPERADMIN --- //
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // SOLO los roles del sistema actual
        string[] roles = { "SuperAdministrador", "Administrador" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"Rol '{role}' creado.");
            }
        }

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

// --- PIPELINE --- //
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

app.MapControllers();

// --- WEBHOOK WHATSAPP --- //
app.MapGet("/api/whatsapp/webhook", (HttpContext context) =>
{
    var hubMode = context.Request.Query["hub.mode"];
    var hubVerifyToken = context.Request.Query["hub.verify_token"];
    var hubChallenge = context.Request.Query["hub.challenge"];
    var verifyToken = "RamsCottons2026Wh";

    if (hubMode == "subscribe" && hubVerifyToken == verifyToken)
    {
        return Results.Content(hubChallenge, "text/plain");
    }

    return Results.BadRequest("Verificacion fallida");
});

app.MapPost("/api/whatsapp/webhook", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    Console.WriteLine($"Mensaje recibido: {body}");

    return Results.Ok();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();