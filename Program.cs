
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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

// BLAZOR SERVER // 
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

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

// AUTENTICACIÓN // 
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddScoped<
    AuthenticationStateProvider,
    IdentityRevalidatingAuthenticationStateProvider>();

// BASE DE DATOS PRINCIPAL// 
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

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

builder.Services.AddHttpClient();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddScoped<SaldoClienteService>();

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

                Console.WriteLine($"Rol '{role}' creado.");
            }
        }

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
// CONSULTA LOCAL DE SALDO
// ==========================================================

if (app.Environment.IsDevelopment())
{
    app.MapGet(
        "/api/pruebas/saldo/{telefono}",
        async (
            string telefono,
            SaldoClienteService saldoService,
            CancellationToken cancellationToken) =>
        {
            var saldo =
                await saldoService.ObtenerSaldoPorTelefonoAsync(
                    telefono,
                    cancellationToken);

            if (!saldo.HasValue)
            {
                return Results.NotFound(new
                {
                    mensaje =
                        "No se encontró saldo para este teléfono."
                });
            }

            return Results.Ok(new
            {
                telefono,
                saldo = saldo.Value
            });
        })
        .RequireAuthorization();
}

// ==========================================================
// WHATSAPP ATENCIÓN
// ==========================================================

app.MapGet(
    "/whatsapp/atencion",
    async (IDbContextFactory<ApplicationDbContext> dbFactory) =>
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

        var numero = Regex.Replace(
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

// WHATSAPP CANAL //
app.MapGet(
    "/whatsapp/canal",
    async (IDbContextFactory<ApplicationDbContext> dbFactory) =>
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
            configuracion.CanalWhatsApp))
        {
            return Results.NotFound(
                "No se ha configurado el canal de WhatsApp.");
        }

        if (!Uri.TryCreate(
            configuracion.CanalWhatsApp,
            UriKind.Absolute,
            out var urlCanal) ||
            (urlCanal.Scheme != Uri.UriSchemeHttp &&
             urlCanal.Scheme != Uri.UriSchemeHttps))
        {
            return Results.BadRequest(
                "La URL del canal de WhatsApp no es válida.");
        }

        return Results.Redirect(urlCanal.ToString());
    });

// WEBHOOK DE WHATSAPP - GET //
// Verificación inicial de Meta //
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
            builder.Configuration["WhatsApp:WebhookVerifyToken"];

        if (string.IsNullOrWhiteSpace(verifyToken))
        {
            verifyToken =
                builder.Configuration["WhatsApp:VerifyToken"];
        }

        if (!string.IsNullOrWhiteSpace(verifyToken) &&
            hubMode == "subscribe" &&
            hubVerifyToken == verifyToken)
        {
            return Results.Content(
                hubChallenge.ToString(),
                "text/plain");
        }

        return Results.BadRequest("Verificación fallida");
    });

// WEBHOOK DE WHATSAPP - POST // 
// Recibe mensajes y respuestas de botones. //
app.MapPost(
    "/api/whatsapp/webhook",
    async (
        HttpContext context,
        WhatsAppService whatsApp,
        SaldoClienteService saldoService,
        IDbContextFactory<ApplicationDbContext> dbFactory) =>
    {
        try
        {
            using var buffer = new MemoryStream();

            await context.Request.Body.CopyToAsync(buffer);

            var cuerpoBytes = buffer.ToArray();

            // Validar que la solicitud procede de Meta.
            var appSecret =
                builder.Configuration["WhatsApp:AppSecret"];

            if (string.IsNullOrWhiteSpace(appSecret))
            {
                Console.WriteLine(
                    "[Webhook] Falta configurar WhatsApp:AppSecret.");

                return Results.StatusCode(503);
            }

            var firmaMeta =
                context.Request.Headers["X-Hub-Signature-256"]
                    .ToString();

            if (!ValidarFirmaMeta(
                cuerpoBytes,
                firmaMeta,
                appSecret))
            {
                Console.WriteLine(
                    "[Webhook] Firma de Meta inválida.");

                return Results.Unauthorized();
            }

            if (cuerpoBytes.Length == 0)
            {
                return Results.Ok();
            }

            using var document =
                JsonDocument.Parse(cuerpoBytes);

            var root = document.RootElement;

            // Respuestas rápidas y botones interactivos, sólo para nuestro
            // número empresarial; firma HMAC ya validada arriba.
            var numeroEmpresa = builder.Configuration["WhatsApp:PhoneNumberId"];
            var respuestas = ObtenerRespuestasRapidas(root, numeroEmpresa);

            // Evitar repetir el mismo mensaje si aparece dos veces en un evento.
            // Para deduplicar reintentos ENTRE webhooks hace falta persistencia.
            var idsEnEstaSolicitud = new HashSet<string>(StringComparer.Ordinal);

            foreach (var respuesta in respuestas)
            {
                if (!string.IsNullOrWhiteSpace(respuesta.IdMensaje) &&
                    !idsEnEstaSolicitud.Add(respuesta.IdMensaje))
                {
                    continue;
                }

                var numero = Regex.Replace(respuesta.Numero, @"\D", "");
                if (numero.Length < 10)
                    continue;

                var ultimos10Boton = numero[^10..];

                if (respuesta.Payload == "RAMS_CONSULTAR_SALDO")
                {
                    await EnviarSaldoActualAsync(
                        numero, ultimos10Boton, saldoService, whatsApp);
                    continue;
                }

                if (respuesta.Payload == "RAMS_TIENDA_ONLINE")
                {
                    var textoTienda =
                        "🛍️ ¡Descubre tus prendas favoritas en *RAMS COTTONS*! 👕👗\n\n" +
                        "Visita nuestra *Tienda Online*: " +
                        "https://www.ramscottons.com.mx/";
                    if (!await whatsApp.EnviarMensajeAsync(numero, textoTienda))
                    {
                        Console.WriteLine(
                            $"[Tienda] Error de envío: {whatsApp.UltimoError}");
                    }
                    continue;
                }

                // No cambiar la lógica existente de consentimiento.
                bool autoriza;
                if (respuesta.Payload == "RAMS_AUTORIZA_SI")
                    autoriza = true;
                else if (respuesta.Payload == "RAMS_AUTORIZA_NO")
                    autoriza = false;
                else
                    continue;

                await using var dbConsentimiento =
                    await dbFactory.CreateDbContextAsync();
                var coincidencias = await dbConsentimiento.Clientes
                    .Where(c =>
                        c.Telefono != null &&
                        c.Telefono.Replace(" ", "")
                            .Replace("-", "")
                            .Replace("+", "")
                            .Replace("(", "")
                            .Replace(")", "")
                            .EndsWith(ultimos10Boton))
                    .Take(2)
                    .ToListAsync();

                if (coincidencias.Count != 1)
                {
                    Console.WriteLine(
                        "[Consentimiento] No se identificó un cliente único.");
                    continue;
                }

                var cliente = coincidencias[0];
                if (cliente.AceptaWhatsApp != autoriza)
                {
                    cliente.AceptaWhatsApp = autoriza;
                    await dbConsentimiento.SaveChangesAsync();
                }

                Console.WriteLine(
                    $"[Consentimiento] Cliente {cliente.Id}: " +
                    $"acepta_whatsapp={(autoriza ? 1 : 0)}; " +
                    $"mensaje={respuesta.IdMensaje}");
            }

            // Si llegan botones y texto en el mismo webhook, atendemos ambos.
            // Comando escrito: "saldo". La plantilla antigua de menú no se envía.
            if (!TryGetIncomingTextMessage(
                root,
                numeroEmpresa,
                out var numeroCliente,
                out var textoMensaje,
                out var messageId))
            {
                return Results.Ok();
            }

            var textoNormalizado = (textoMensaje ?? string.Empty)
                .Trim().ToLowerInvariant();

            if (textoNormalizado != "saldo")
                return Results.Ok();

            var numeroNormalizado = Regex.Replace(
                numeroCliente ?? string.Empty, @"\D", "");
            if (numeroNormalizado.Length < 10)
                return Results.Ok();

            var ultimos10 = numeroNormalizado[^10..];
            Console.WriteLine($"[Saldo] Solicitud escrita: {messageId}");
            await EnviarSaldoActualAsync(
                numeroNormalizado, ultimos10, saldoService, whatsApp);
            return Results.Ok();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(
                $"[Consentimiento] Error SQL: {ex.Message}");

            return Results.StatusCode(500);
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

            return Results.StatusCode(500);
        }
    });

// ==========================================================
// RESPUESTA DE SALDO (texto, respuesta rápida e interactiva)
// Envía de nuevo un botón "Saldo" para consultas sucesivas.
// ==========================================================
static async Task EnviarSaldoActualAsync(
    string numeroDestino,
    string telefono10,
    SaldoClienteService saldoService,
    WhatsAppService whatsApp)
{
    var saldo = await saldoService.ObtenerSaldoPorTelefonoAsync(telefono10);
    string respuesta;

    if (saldo.HasValue)
    {
        var saldoFormateado = saldo.Value.ToString(
            "N0", CultureInfo.GetCultureInfo("es-MX"));

        respuesta =
            "Te compartimos el saldo actual de tus *PUNTOS RAMS*: " +
            $"*${saldoFormateado} pesos*. 🎉\n\n" +
            "¡Sigue acumulando puntos con tus compras y disfruta " +
            "de más descuentos con *RAMS COTTONS*! 👕👗\n\n" +
            "¡Aprovecha tus beneficios! 🌐 https://ramscottons.com.mx/";
    }
    else
    {
        respuesta =
            "🛍️ *RAMS COTTONS*\n\n" +
            "No encontramos puntos registrados para tu número telefónico.\n\n" +
            "Comunícate con uno de nuestros asesores para recibir atención.";
    }

    var enviado = await whatsApp.EnviarSaldoConBotonAsync(
        numeroDestino, respuesta);
    if (!enviado)
    {
        Console.WriteLine($"[Saldo] Error de envío: {whatsApp.UltimoError}");
    }
}

// VALIDAR FIRMA HMAC DE META //
static bool ValidarFirmaMeta(
    byte[] cuerpo,
    string firma,
    string appSecret)
{
    const string prefijo = "sha256=";

    if (string.IsNullOrWhiteSpace(firma) ||
        !firma.StartsWith(
            prefijo,
            StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    var hex = firma[prefijo.Length..];

    if (hex.Length != 64)
    {
        return false;
    }

    try
    {
        var firmaRecibida = Convert.FromHexString(hex);

        using var hmac = new HMACSHA256(
            Encoding.UTF8.GetBytes(appSecret));

        var firmaEsperada = hmac.ComputeHash(cuerpo);

        return CryptographicOperations.FixedTimeEquals(
            firmaEsperada,
            firmaRecibida);
    }
    catch (FormatException)
    {
        return false;
    }
}

// LEER RESPUESTAS RÁPIDAS // 

static List<(string Numero, string Texto, string Payload, string IdMensaje)>
    ObtenerRespuestasRapidas(JsonElement root, string? numeroEmpresa)
{
    var resultado =
        new List<(string Numero, string Texto, string Payload, string IdMensaje)>();

    if (string.IsNullOrWhiteSpace(numeroEmpresa) ||
        !root.TryGetProperty("entry", out var entries) ||
        entries.ValueKind != JsonValueKind.Array)
        return resultado;

    foreach (var entry in entries.EnumerateArray())
    {
        if (!entry.TryGetProperty("changes", out var changes) ||
            changes.ValueKind != JsonValueKind.Array)
            continue;

        foreach (var change in changes.EnumerateArray())
        {
            if (!change.TryGetProperty("value", out var value) ||
                !value.TryGetProperty("metadata", out var metadata) ||
                !metadata.TryGetProperty("phone_number_id", out var idEmpresa) ||
                idEmpresa.GetString() != numeroEmpresa ||
                !value.TryGetProperty("messages", out var messages) ||
                messages.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var message in messages.EnumerateArray())
            {
                if (!message.TryGetProperty("type", out var tipo))
                    continue;

                string? texto = null;
                string? payload = null;

                if (tipo.GetString() == "button" &&
                    message.TryGetProperty("button", out var boton))
                {
                    texto = boton.TryGetProperty("text", out var t)
                        ? t.GetString() : null;
                    payload = boton.TryGetProperty("payload", out var p)
                        ? p.GetString() : null;
                }
                else if (tipo.GetString() == "interactive" &&
                    message.TryGetProperty("interactive", out var interactivo) &&
                    interactivo.TryGetProperty("type", out var subTipo) &&
                    subTipo.GetString() == "button_reply" &&
                    interactivo.TryGetProperty("button_reply", out var botonInteractivo))
                {
                    texto = botonInteractivo.TryGetProperty("title", out var t)
                        ? t.GetString() : null;
                    payload = botonInteractivo.TryGetProperty("id", out var p)
                        ? p.GetString() : null;
                }
                else
                    continue;

                var numero = message.TryGetProperty("from", out var desde)
                    ? desde.GetString() : null;
                var id = message.TryGetProperty("id", out var identificador)
                    ? identificador.GetString() : null;

                if (!string.IsNullOrWhiteSpace(numero) &&
                    !string.IsNullOrWhiteSpace(payload))
                {
                    resultado.Add((numero, texto ?? "", payload, id ?? ""));
                }
            }
        }
    }
    return resultado;
}

// LEER MENSAJES DE TEXTO //
static bool TryGetIncomingTextMessage(
    JsonElement root,
    string? numeroEmpresa,
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
            "entry", out var entries) ||
            entries.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty(
                "changes", out var changes) ||
                changes.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var change in changes.EnumerateArray())
            {
                if (!change.TryGetProperty(
                    "value", out var value))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(numeroEmpresa) ||
                    !value.TryGetProperty("metadata", out var metadata) ||
                    !metadata.TryGetProperty("phone_number_id", out var idEmpresa) ||
                    idEmpresa.GetString() != numeroEmpresa ||
                    !value.TryGetProperty(
                        "messages", out var messages) ||
                    messages.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var message in messages.EnumerateArray())
                {
                    if (!message.TryGetProperty(
                        "type", out var typeElement) ||
                        typeElement.GetString() != "text")
                    {
                        continue;
                    }

                    if (message.TryGetProperty(
                        "from", out var fromElement))
                    {
                        numeroCliente = fromElement.GetString();
                    }

                    if (message.TryGetProperty(
                        "id", out var idElement))
                    {
                        messageId = idElement.GetString();
                    }

                    if (message.TryGetProperty(
                        "text", out var textElement) &&
                        textElement.TryGetProperty(
                            "body", out var bodyElement))
                    {
                        textoMensaje = bodyElement.GetString();
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

//  COMPONENTES RAZOR //
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// IDENTITY //
app.MapAdditionalIdentityEndpoints();

app.Run();
