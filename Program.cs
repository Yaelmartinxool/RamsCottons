using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RamsCottons.Components;
using RamsCottons.Components.Account;
using RamsCottons.Data;
using RamsCottons.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// RAZOR / BLAZOR SERVER
// ==========================================================
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

// ==========================================================
// ARCHIVOS GRANDES - HASTA 100 MB
// ==========================================================
builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 100 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddControllers();

// ==========================================================
// AUTENTICACIÓN BLAZOR
// ==========================================================
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<
    AuthenticationStateProvider,
    IdentityRevalidatingAuthenticationStateProvider>();

// ==========================================================
// BASE DE DATOS PRINCIPAL
// ==========================================================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

// IMPORTANTE:
// Usamos Factory para evitar compartir el mismo DbContext
// en operaciones simultáneas de Blazor Server.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
    });
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ==========================================================
// CYCDATA - SOLO LECTURA
// ==========================================================
var cycDataConnectionString =
    builder.Configuration.GetConnectionString("CycDataConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'CycDataConnection' not found.");

builder.Services.AddDbContext<CycDataContext>(options =>
{
    options.UseSqlServer(cycDataConnectionString, sqlOptions =>
    {
        sqlOptions.MigrationsHistoryTable(
            "__EFMigrationsHistory_CycData");
    });
});

// ==========================================================
// IDENTITY
// ==========================================================
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
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

// ==========================================================
// SERVICIOS
// ==========================================================
builder.Services.AddTransient<
    IEmailSender<ApplicationUser>,
    EmailSender>();

builder.Services.AddScoped<QrGeneratorService>();
builder.Services.AddScoped<PermisoService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<SucursalService>();
builder.Services.AddScoped<UserSucursalService>();

// ==========================================================
// WHATSAPP
// ==========================================================
builder.Services.AddHttpClient();
builder.Services.AddScoped<WhatsAppService>();

// ==========================================================
// AUTORIZACIÓN
// ==========================================================
builder.Services.AddAuthorization();

var app = builder.Build();

// ==========================================================
// CREAR ROLES Y SUPERADMIN
// ==========================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles =
        {
            "SuperAdministrador",
            "Administrador"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));

                Console.WriteLine(
                    $"Rol '{role}' creado.");
            }
        }

        // Se recomienda guardar estas credenciales
        // en configuración segura / User Secrets.
        var adminEmail =
            builder.Configuration["BootstrapAdmin:Email"];

        var adminPassword =
            builder.Configuration["BootstrapAdmin:Password"];

        if (!string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

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

                var createResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "SuperAdministrador");

                    Console.WriteLine(
                        $"SuperAdministrador creado.");
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        Console.WriteLine(
                            $"Error: {error.Description}");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Error inicializando: {ex.Message}");
    }
}

// ==========================================================
// PIPELINE
// ==========================================================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

// ==========================================================
// WEBHOOK WHATSAPP - GET
// ==========================================================
app.MapGet(
    "/api/whatsapp/webhook",
    (HttpContext context) =>
    {
        var hubMode =
            context.Request.Query["hub.mode"];

        var hubVerifyToken =
            context.Request.Query["hub.verify_token"];

        var hubChallenge =
            context.Request.Query["hub.challenge"];

        var verifyToken =
            builder.Configuration[
                "WhatsApp:WebhookVerifyToken"];

        if (!string.IsNullOrWhiteSpace(verifyToken) &&
            hubMode == "subscribe" &&
            hubVerifyToken == verifyToken)
        {
            return Results.Content(
                hubChallenge,
                "text/plain");
        }

        return Results.BadRequest(
            "Verificacion fallida");
    });

// ==========================================================
// WEBHOOK WHATSAPP - POST
// ==========================================================
app.MapPost(
    "/api/whatsapp/webhook",
    async (HttpContext context) =>
    {
        using var reader =
            new StreamReader(
                context.Request.Body);

        var body =
            await reader.ReadToEndAsync();

        Console.WriteLine(
            $"Mensaje recibido: {body}");

        return Results.Ok();
    });

// ==========================================================
// RAZOR COMPONENTS
// ==========================================================
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ==========================================================
// IDENTITY ENDPOINTS
// ==========================================================
app.MapAdditionalIdentityEndpoints();

app.Run();