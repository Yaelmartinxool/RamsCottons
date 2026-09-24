using System.Text.Json;
using System.Text.RegularExpressions;
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
                        "SuperAdministrador creado.");
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
// WHATSAPP ATENCION - REDIRECCION A NUMERO DE BD
// ==========================================================
app.MapGet(
    "/whatsapp/atencion",
    async (
        IDbContextFactory<ApplicationDbContext> dbFactory) =>
    {
        await using var db =
            await dbFactory.CreateDbContextAsync();

        var configuracion =
            await db.ConfiguracionWhatsApp
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Activo);

        if (configuracion == null)
        {
            return Results.NotFound(
                "No existe una configuración activa de WhatsApp.");
        }

        if (string.IsNullOrWhiteSpace(
            configuracion.WhatsAppAtencion))
        {
            return Results.NotFound(
                "No se ha configurado el número de atención por WhatsApp.");
        }

        // Ejemplo:
        // +52 1 999 442 6671
        // ↓
        // 5219994426671
        var numero =
            Regex.Replace(
                configuracion.WhatsAppAtencion,
                @"\D",
                "");

        if (string.IsNullOrWhiteSpace(numero))
        {
            return Results.BadRequest(
                "El número de WhatsApp de atención no es válido.");
        }

        var mensaje =
            "Hola 😊 Me gustaría hablar con un asesor de Rams Cottons.";

        var urlWhatsApp =
            $"https://wa.me/{numero}?text={Uri.EscapeDataString(mensaje)}";

        return Results.Redirect(urlWhatsApp);
    });

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
// SI EL CLIENTE ESCRIBE "menu" O "menú",
// ENVÍA AUTOMÁTICAMENTE LA PLANTILLA menu_atencion_rams
// ==========================================================
app.MapPost(
    "/api/whatsapp/webhook",
    async (
        HttpContext context,
        WhatsAppService whatsApp,
        IDbContextFactory<ApplicationDbContext> dbFactory) =>
    {
        try
        {
            using var reader =
                new StreamReader(
                    context.Request.Body);

            var body =
                await reader.ReadToEndAsync();

            Console.WriteLine(
                $"Mensaje recibido: {body}");

            if (string.IsNullOrWhiteSpace(body))
            {
                return Results.Ok();
            }

            using var document =
                JsonDocument.Parse(body);

            var root =
                document.RootElement;

            // Los estados de mensajes (sent, delivered, read, etc.)
            // también llegan al webhook. Si no hay un mensaje de texto
            // entrante, simplemente respondemos OK.
            if (!TryGetIncomingTextMessage(
                root,
                out var numeroCliente,
                out var textoMensaje,
                out var messageId))
            {
                return Results.Ok();
            }

            Console.WriteLine(
                $"[Webhook] Mensaje entrante {messageId} de {numeroCliente}: {textoMensaje}");

            var textoNormalizado =
                (textoMensaje ?? string.Empty)
                    .Trim()
                    .ToLowerInvariant();

            // Por ahora solo respondemos a MENU / MENÚ.
            if (textoNormalizado != "menu" &&
                textoNormalizado != "menú")
            {
                return Results.Ok();
            }

            var numeroNormalizado =
                Regex.Replace(
                    numeroCliente ?? string.Empty,
                    @"\D",
                    "");

            if (string.IsNullOrWhiteSpace(numeroNormalizado))
            {
                Console.WriteLine(
                    "[Webhook] No se pudo obtener un número válido.");

                return Results.Ok();
            }

            // Para localizar al cliente en la BD usamos los últimos
            // 10 dígitos, independientemente de si está guardado con
            // +52, espacios, guiones, paréntesis, etc.
            var ultimos10 =
                numeroNormalizado.Length > 10
                    ? numeroNormalizado[^10..]
                    : numeroNormalizado;

            await using var db =
                await dbFactory.CreateDbContextAsync();

            var cliente =
                await db.Clientes
                    .AsNoTracking()
                    .Where(c =>
                        c.Telefono != null &&
                        c.Telefono
                            .Replace(" ", "")
                            .Replace("-", "")
                            .Replace("+", "")
                            .Replace("(", "")
                            .Replace(")", "")
                            .EndsWith(ultimos10))
                    .FirstOrDefaultAsync();

            var primerNombre =
                cliente?.NombreCompleto?
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

            // Si el número aún no está registrado como cliente,
            // el menú se sigue enviando con un saludo genérico.
            if (string.IsNullOrWhiteSpace(primerNombre))
            {
                primerNombre = "Cliente";
            }

            var resultado =
                await whatsApp.EnviarPlantillaAsync(
                    numeroDestino: numeroNormalizado,
                    nombrePlantilla: "menu_atencion_rams",
                    idioma: "es_MX",
                    parametros: new Dictionary<string, string>
                    {
                        { "1", primerNombre }
                    });

            if (resultado)
            {
                Console.WriteLine(
                    $"[Webhook] Menú enviado correctamente a {numeroNormalizado}.");
            }
            else
            {
                Console.WriteLine(
                    $"[Webhook] Error al enviar menú a {numeroNormalizado}: {whatsApp.UltimoError}");
            }

            // Confirmamos a Meta que el webhook fue recibido.
            return Results.Ok();
        }
        catch (JsonException ex)
        {
            Console.WriteLine(
                $"[Webhook] JSON inválido: {ex.Message}");

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Webhook] Error: {ex.Message}");

            return Results.Ok();
        }
    });

// ==========================================================
// LECTOR DE MENSAJES ENTRANTES DE WHATSAPP
// ==========================================================
static bool TryGetIncomingTextMessage(
    JsonElement root,
    out string? numeroCliente,
    out string? textoMensaje,
    out string? messageId)
{
    numeroCliente = null;
    textoMensaje = null;
    messageId = null;

    try
    {
        if (!root.TryGetProperty(
                "entry",
                out var entries) ||
            entries.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty(
                    "changes",
                    out var changes) ||
                changes.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var change in changes.EnumerateArray())
            {
                if (!change.TryGetProperty(
                        "value",
                        out var value))
                {
                    continue;
                }

                if (!value.TryGetProperty(
                        "messages",
                        out var messages) ||
                    messages.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var message in messages.EnumerateArray())
                {
                    if (!message.TryGetProperty(
                            "type",
                            out var typeElement))
                    {
                        continue;
                    }

                    var tipo =
                        typeElement.GetString();

                    if (!string.Equals(
                            tipo,
                            "text",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (message.TryGetProperty(
                        "from",
                        out var fromElement))
                    {
                        numeroCliente =
                            fromElement.GetString();
                    }

                    if (message.TryGetProperty(
                        "id",
                        out var idElement))
                    {
                        messageId =
                            idElement.GetString();
                    }

                    if (message.TryGetProperty(
                            "text",
                            out var textElement) &&
                        textElement.TryGetProperty(
                            "body",
                            out var bodyElement))
                    {
                        textoMensaje =
                            bodyElement.GetString();
                    }

                    return
                        !string.IsNullOrWhiteSpace(numeroCliente) &&
                        !string.IsNullOrWhiteSpace(textoMensaje);
                }
            }
        }
    }
    catch
    {
        return false;
    }

    return false;
}

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