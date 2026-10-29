using System.Text;
using System.Text.Json;
namespace RamsCottons.Services;
public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiVersion = "v25.0";
    public string? UltimoError { get; private set; }
    public WhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    // =========================================================
    // MENSAJE DE TEXTO
    // Solo funciona dentro de la ventana de 24 horas
    // =========================================================
    public async Task<bool> EnviarMensajeAsync(
        string numeroDestino,
        string mensaje)
    {
        try
        {
            var accessToken =
                _configuration["WhatsApp:AccessToken"];
            var phoneNumberId =
                _configuration["WhatsApp:PhoneNumberId"];
            if (
                string.IsNullOrEmpty(accessToken) ||
                string.IsNullOrEmpty(phoneNumberId)
            )
            {
                Console.WriteLine(
                    "Configuracion de WhatsApp incompleta"
                );
                UltimoError =
                    "Configuracion de WhatsApp incompleta";
                return false;
            }
            var url =
                $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "text",
                text = new
                {
                    body = mensaje
                }
            };
            var json =
                JsonSerializer.Serialize(payload);
            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );
            _httpClient
                .DefaultRequestHeaders
                .Clear();
            _httpClient
                .DefaultRequestHeaders
                .Add(
                    "Authorization",
                    $"Bearer {accessToken}"
                );
            var response =
                await _httpClient.PostAsync(
                    url,
                    content
                );
            var responseContent =
                await response.Content
                    .ReadAsStringAsync();
            Console.WriteLine(
                "========================================"
            );
            Console.WriteLine(
                $"[WhatsApp] Mensaje de texto a: {numeroDestino}"
            );
            Console.WriteLine(
                $"[WhatsApp] Status: {response.StatusCode}"
            );
            Console.WriteLine(
                $"[WhatsApp] Response: {responseContent}"
            );
            Console.WriteLine(
                "========================================"
            );
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Mensaje enviado a {numeroDestino}"
                );
                UltimoError = null;
                return true;
            }
            Console.WriteLine(
                $"Error: {responseContent}"
            );
            UltimoError = responseContent;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Excepcion: {ex.Message}"
            );
            UltimoError = ex.Message;
            return false;
        }
    }
    // =========================================================
    // SALDO CON BOTÓN REUTILIZABLE (ventana de 24 horas)
    // El usuario puede pulsar "Saldo" de nuevo cuando lo necesite.
    // =========================================================
    public async Task<bool> EnviarSaldoConBotonAsync(
        string numeroDestino,
        string mensaje)
    {
        try
        {
            UltimoError = null;
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrWhiteSpace(accessToken) ||
                string.IsNullOrWhiteSpace(phoneNumberId))
            {
                UltimoError = "Configuración de WhatsApp incompleta.";
                return false;
            }

            var url =
                $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "interactive",
                interactive = new
                {
                    type = "button",
                    body = new { text = mensaje },
                    action = new
                    {
                        buttons = new[]
                        {
                            new
                            {
                                type = "reply",
                                reply = new
                                {
                                    id = "RAMS_CONSULTAR_SALDO",
                                    title = "Saldo"
                                }
                            }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var resultado = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                UltimoError = resultado;
                Console.WriteLine($"[Saldo/WhatsApp] Error: {resultado}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            UltimoError = ex.Message;
            Console.WriteLine($"[Saldo/WhatsApp] Excepción: {ex.Message}");
            return false;
        }
    }

    // =========================================================
    // PROMOCION COMO TEXTO
    // Solo funciona dentro de ventana de 24 horas
    // =========================================================
    public async Task<bool> EnviarPromocionAsync(
        string numeroDestino,
        string titulo,
        string descripcion,
        string? codigo = null)
    {
        var mensaje =
            $" {titulo}\n\n{descripcion}";
        if (!string.IsNullOrEmpty(codigo))
        {
            mensaje +=
                $"\n\nCodigo: {codigo}";
        }
        mensaje +=
            "\n\nValido hoy!";
        return await EnviarMensajeAsync(
            numeroDestino,
            mensaje
        );
    }
    // =========================================================
    // PLANTILLA NORMAL
    // Se usa para plantillas SIN imagen en encabezado
    // =========================================================
    public async Task<bool> EnviarPlantillaAsync(
        string numeroDestino,
        string nombrePlantilla,
        string idioma,
        Dictionary<string, string> parametros)
    {
        try
        {
            var accessToken =
                _configuration["WhatsApp:AccessToken"];
            var phoneNumberId =
                _configuration["WhatsApp:PhoneNumberId"];
            if (
                string.IsNullOrEmpty(accessToken) ||
                string.IsNullOrEmpty(phoneNumberId)
            )
            {
                Console.WriteLine(
                    "Configuracion de WhatsApp incompleta"
                );
                UltimoError =
                    "Configuracion de WhatsApp incompleta (AccessToken o PhoneNumberId)";
                return false;
            }
            var url =
                $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
            var parametrosOrdenados =
                parametros
                    .OrderBy(p =>
                    {
                        return int.TryParse(
                            p.Key,
                            out var numero
                        )
                            ? numero
                            : int.MaxValue;
                    })
                    .Select(p => new
                    {
                        type = "text",
                        text = p.Value
                    })
                    .ToList();
            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new
                    {
                        code = idioma
                    },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = parametrosOrdenados
                        }
                    }
                }
            };
            var json =
                JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );
            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );
            _httpClient
                .DefaultRequestHeaders
                .Clear();
            _httpClient
                .DefaultRequestHeaders
                .Add(
                    "Authorization",
                    $"Bearer {accessToken}"
                );
            var response =
                await _httpClient.PostAsync(
                    url,
                    content
                );
            var responseContent =
                await response.Content
                    .ReadAsStringAsync();
            Console.WriteLine(
                "========================================"
            );
            Console.WriteLine(
                $"[WhatsApp] Enviando plantilla: {nombrePlantilla}"
            );
            Console.WriteLine(
                $"[WhatsApp] Destino: {numeroDestino}"
            );
            Console.WriteLine(
                $"[WhatsApp] Idioma: {idioma}"
            );
            Console.WriteLine(
                $"[WhatsApp] Status: {response.StatusCode}"
            );
            Console.WriteLine(
                $"[WhatsApp] Response: {responseContent}"
            );
            Console.WriteLine(
                "========================================"
            );
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Plantilla '{nombrePlantilla}' enviada a {numeroDestino}"
                );
                UltimoError = null;
                return true;
            }
            Console.WriteLine(
                $"Error al enviar plantilla: {responseContent}"
            );
            UltimoError = responseContent;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Excepcion al enviar plantilla: {ex.Message}"
            );
            UltimoError = ex.Message;
            return false;
        }
    }
    // =========================================================
    // PLANTILLA CON BOTON URL
    // =========================================================
    public async Task<bool> EnviarPlantillaConBotonAsync(
        string numeroDestino,
        string nombrePlantilla,
        string idioma,
        Dictionary<string, string> parametros,
        string urlBoton)
    {
        try
        {
            var accessToken =
                _configuration["WhatsApp:AccessToken"];
            var phoneNumberId =
                _configuration["WhatsApp:PhoneNumberId"];
            if (
                string.IsNullOrEmpty(accessToken) ||
                string.IsNullOrEmpty(phoneNumberId)
            )
            {
                Console.WriteLine(
                    "Configuracion de WhatsApp incompleta"
                );
                UltimoError =
                    "Configuracion de WhatsApp incompleta";
                return false;
            }
            var url =
                $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
            var parametrosOrdenados =
                parametros
                    .OrderBy(p =>
                    {
                        return int.TryParse(
                            p.Key,
                            out var numero
                        )
                            ? numero
                            : int.MaxValue;
                    })
                    .Select(p => new
                    {
                        type = "text",
                        text = p.Value
                    })
                    .ToList();
            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new
                    {
                        code = idioma
                    },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = parametrosOrdenados
                        },
                        new
                        {
                            type = "button",
                            sub_type = "url",
                            index = 0,
                            parameters = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = urlBoton
                                }
                            }
                        }
                    }
                }
            };
            var json =
                JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );
            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );
            _httpClient
                .DefaultRequestHeaders
                .Clear();
            _httpClient
                .DefaultRequestHeaders
                .Add(
                    "Authorization",
                    $"Bearer {accessToken}"
                );
            var response =
                await _httpClient.PostAsync(
                    url,
                    content
                );
            var responseContent =
                await response.Content
                    .ReadAsStringAsync();
            Console.WriteLine(
                "========================================"
            );
            Console.WriteLine(
                $"[WhatsApp] Plantilla con boton: {nombrePlantilla}"
            );
            Console.WriteLine(
                $"[WhatsApp] Destino: {numeroDestino}"
            );
            Console.WriteLine(
                $"[WhatsApp] Status: {response.StatusCode}"
            );
            Console.WriteLine(
                $"[WhatsApp] Response: {responseContent}"
            );
            Console.WriteLine(
                "========================================"
            );
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Plantilla con boton '{nombrePlantilla}' enviada a {numeroDestino}"
                );
                UltimoError = null;
                return true;
            }
            Console.WriteLine(
                $"Error al enviar plantilla con boton: {responseContent}"
            );
            UltimoError = responseContent;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Excepcion al enviar plantilla con boton: {ex.Message}"
            );
            UltimoError = ex.Message;
            return false;
        }
    }
    // =========================================================
    // MENSAJE CON IMAGEN NORMAL
    // Dentro de ventana de 24 horas
    // =========================================================
    public async Task<bool> EnviarMensajeConImagenAsync(
        string numeroDestino,
        string mensaje,
        string imagenUrl)
    {
        try
        {
            var accessToken =
                _configuration["WhatsApp:AccessToken"];
            var phoneNumberId =
                _configuration["WhatsApp:PhoneNumberId"];
            if (
                string.IsNullOrEmpty(accessToken) ||
                string.IsNullOrEmpty(phoneNumberId)
            )
            {
                Console.WriteLine(
                    "Configuracion de WhatsApp incompleta"
                );
                UltimoError =
                    "Configuracion de WhatsApp incompleta";
                return false;
            }
            var url =
                $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "image",
                image = new
                {
                    link = imagenUrl,
                    caption = mensaje
                }
            };
            var json =
                JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );
            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );
            _httpClient
                .DefaultRequestHeaders
                .Clear();
            _httpClient
                .DefaultRequestHeaders
                .Add(
                    "Authorization",
                    $"Bearer {accessToken}"
                );
            var response =
                await _httpClient.PostAsync(
                    url,
                    content
                );
            var responseContent =
                await response.Content
                    .ReadAsStringAsync();
            Console.WriteLine(
                "========================================"
            );
            Console.WriteLine(
                $"[WhatsApp] Mensaje con imagen a: {numeroDestino}"
            );
            Console.WriteLine(
                $"[WhatsApp] Status: {response.StatusCode}"
            );
            Console.WriteLine(
                $"[WhatsApp] Response: {responseContent}"
            );
            Console.WriteLine(
                "========================================"
            );
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Mensaje con imagen enviado a {numeroDestino}"
                );
                UltimoError = null;
                return true;
            }
            Console.WriteLine(
                $"Error: {responseContent}"
            );
            UltimoError = responseContent;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Excepcion: {ex.Message}"
            );
            UltimoError = ex.Message;
            return false;
        }
    }
    // =========================================================
    // PLANTILLA CON IMAGEN EN HEADER
    //
    // ESTA ES LA QUE DEBES USAR PARA:
    // "bienvenida_rams_cliente_v2"
    // =========================================================
    public async Task<bool> EnviarPlantillaConImagenAsync(
        string numeroDestino,
        string nombrePlantilla,
        string idioma,
        Dictionary<string, string> parametros,
        string imagenUrl)
    {
        try
        {
            var accessToken =
                _configuration["WhatsApp:AccessToken"];
            var phoneNumberId =
                _configuration["WhatsApp:PhoneNumberId"];
            if (
                string.IsNullOrEmpty(accessToken) ||
                string.IsNullOrEmpty(phoneNumberId)
            )
            {
                Console.WriteLine(
                    "Configuracion de WhatsApp incompleta"
                );
                UltimoError =
                    "Configuracion de WhatsApp incompleta";
                return false;
            }
            if (string.IsNullOrWhiteSpace(imagenUrl))
            {
                Console.WriteLine(
                    "La URL de la imagen esta vacia"
                );
                UltimoError =
                    "La URL de la imagen esta vacia";
                return false;
            }
            var url =
                $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
            var parametrosOrdenados =
                parametros
                    .OrderBy(p =>
                    {
                        return int.TryParse(
                            p.Key,
                            out var numero
                        )
                            ? numero
                            : int.MaxValue;
                    })
                    .Select(p => new
                    {
                        type = "text",
                        text = p.Value
                    })
                    .ToList();
            // Dos enlaces estáticos en Meta (índices 0 y 1):
            // "Seguir canal" y "Hablar con un asesor".
            // No necesitan parámetros en el envío.
            var componentes = new List<object>
            {
                new
                {
                    type = "header",
                    parameters = new object[]
                    {
                        new { type = "image", image = new { link = imagenUrl } }
                    }
                },
                new
                {
                    type = "body",
                    parameters = parametrosOrdenados
                }
            };

            // Respuestas rápidas de bienvenida (índices 2 y 3 en Meta).
            // Sólo se agregan a esta plantilla, para no alterar las demás.
            if (string.Equals(nombrePlantilla, "bienvenida_rams_cliente_v2",
                StringComparison.OrdinalIgnoreCase))
            {
                componentes.Add(new
                {
                    type = "button",
                    sub_type = "quick_reply",
                    index = "2", // Tienda Online (después de los 2 botones URL)
                    parameters = new[]
                    {
                        new { type = "payload", payload = "RAMS_TIENDA_ONLINE" }
                    }
                });
                componentes.Add(new
                {
                    type = "button",
                    sub_type = "quick_reply",
                    index = "3", // Saldo
                    parameters = new[]
                    {
                        new { type = "payload", payload = "RAMS_CONSULTAR_SALDO" }
                    }
                });
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new { code = idioma },
                    components = componentes.ToArray()
                }
            };
            var json =
                JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );
            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );
            _httpClient
                .DefaultRequestHeaders
                .Clear();
            _httpClient
                .DefaultRequestHeaders
                .Add(
                    "Authorization",
                    $"Bearer {accessToken}"
                );
            Console.WriteLine(
                "========================================"
            );
            Console.WriteLine(
                $"[WhatsApp] Enviando plantilla CON IMAGEN: {nombrePlantilla}"
            );
            Console.WriteLine(
                $"[WhatsApp] Destino: {numeroDestino}"
            );
            Console.WriteLine(
                $"[WhatsApp] Idioma: {idioma}"
            );
            Console.WriteLine(
                $"[WhatsApp] Imagen: {imagenUrl}"
            );
            Console.WriteLine(
                $"[WhatsApp] Payload: {json}"
            );
            var response =
                await _httpClient.PostAsync(
                    url,
                    content
                );
            var responseContent =
                await response.Content
                    .ReadAsStringAsync();
            Console.WriteLine(
                $"[WhatsApp] Status: {response.StatusCode}"
            );
            Console.WriteLine(
                $"[WhatsApp] Response: {responseContent}"
            );
            Console.WriteLine(
                "========================================"
            );
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Plantilla con imagen '{nombrePlantilla}' enviada a {numeroDestino}"
                );
                UltimoError = null;
                return true;
            }
            Console.WriteLine(
                $"Error al enviar plantilla con imagen: {responseContent}"
            );
            UltimoError = responseContent;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error al enviar plantilla con imagen: {ex.Message}"
            );
            UltimoError = ex.Message;
            return false;
        }
    }
public async Task<bool> EnviarSolicitudAutorizacionAsync(
    string numeroDestino)
{
    try
    {
        UltimoError = null;
        var accessToken =
            _configuration["WhatsApp:AccessToken"];
        var phoneNumberId =
            _configuration["WhatsApp:PhoneNumberId"];
        if (string.IsNullOrWhiteSpace(accessToken) ||
            string.IsNullOrWhiteSpace(phoneNumberId))
        {
            UltimoError = "Configuración de WhatsApp incompleta.";
            return false;
        }
        // El número debe incluir código de país, solo dígitos.
        if (string.IsNullOrWhiteSpace(numeroDestino) ||
            !numeroDestino.All(char.IsDigit))
        {
            UltimoError = "Número de destino inválido.";
            return false;
        }
        var url =
            $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";
        var payload = new
        {
            messaging_product = "whatsapp",
            to = numeroDestino,
            type = "template",
            template = new
            {
                name = "autorizacion_promociones_rams",
                language = new
                {
                    code = "es_MX"
                },
                components = new object[]
                {
                    new
                    {
                        type = "button",
                        sub_type = "quick_reply",
                        index = "0",
                        parameters = new[]
                        {
                            new
                            {
                                type = "payload",
                                payload = "RAMS_AUTORIZA_SI"
                            }
                        }
                    },
                    new
                    {
                        type = "button",
                        sub_type = "quick_reply",
                        index = "1",
                        parameters = new[]
                        {
                            new
                            {
                                type = "payload",
                                payload = "RAMS_AUTORIZA_NO"
                            }
                        }
                    }
                }
            }
        };
        using var request = new HttpRequestMessage(
            HttpMethod.Post, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", accessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
        using var response =
            await _httpClient.SendAsync(request);
        var respuesta =
            await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            UltimoError = respuesta;
            Console.WriteLine(
                $"[WhatsApp] Error de plantilla: {respuesta}");
            return false;
        }
        Console.WriteLine(
            "[WhatsApp] Solicitud de autorización aceptada por Meta.");
        return true;
    }
    catch (Exception ex)
    {
        UltimoError = ex.Message;
        Console.WriteLine(
            $"[WhatsApp] Error: {ex.Message}");
        return false;
    }
}
}
