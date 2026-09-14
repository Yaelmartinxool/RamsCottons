using System.Text;
using System.Text.Json;

namespace RamsCottons.Services;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiVersion = "v19.0";

    // ✅ NUEVO: Guardar el último error para mostrarlo al usuario
    public string? UltimoError { get; private set; }

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> EnviarMensajeAsync(string numeroDestino, string mensaje)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                Console.WriteLine("Configuracion de WhatsApp incompleta");
                UltimoError = "Configuracion de WhatsApp incompleta";
                return false;
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "text",
                text = new { body = mensaje }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Mensaje enviado a {numeroDestino}");
                UltimoError = null;
                return true;
            }
            else
            {
                Console.WriteLine($"Error: {responseContent}");
                UltimoError = responseContent;
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepcion: {ex.Message}");
            UltimoError = ex.Message;
            return false;
        }
    }

    public async Task<bool> EnviarPromocionAsync(string numeroDestino, string titulo, string descripcion, string? codigo = null)
    {
        var mensaje = $" {titulo}\n\n{descripcion}";
        
        if (!string.IsNullOrEmpty(codigo))
            mensaje += $"\n\nCodigo: {codigo}";
        
        mensaje += "\n\nValido hoy!";

        return await EnviarMensajeAsync(numeroDestino, mensaje);
    }

    public async Task<bool> EnviarPlantillaAsync(string numeroDestino, string nombrePlantilla, string idioma, Dictionary<string, string> parametros)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                Console.WriteLine("Configuracion de WhatsApp incompleta");
                UltimoError = "Configuracion de WhatsApp incompleta (AccessToken o PhoneNumberId)";
                return false;
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new { code = idioma },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = parametros.Select(p => new
                            {
                                type = "text",
                                text = p.Value
                            }).ToList()
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // ✅ LOGS en consola para debug local
            Console.WriteLine("========================================");
            Console.WriteLine($"[WhatsApp] Enviando plantilla: {nombrePlantilla}");
            Console.WriteLine($"[WhatsApp] Destino: {numeroDestino}");
            Console.WriteLine($"[WhatsApp] Idioma: {idioma}");
            Console.WriteLine($"[WhatsApp] Status: {response.StatusCode}");
            Console.WriteLine($"[WhatsApp] Response: {responseContent}");
            Console.WriteLine("========================================");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Plantilla '{nombrePlantilla}' enviada a {numeroDestino}");
                UltimoError = null;
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Error al enviar plantilla: {responseContent}");
                UltimoError = responseContent;
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepcion al enviar plantilla: {ex.Message}");
            UltimoError = ex.Message;
            return false;
        }
    }

    public async Task<bool> EnviarPlantillaConBotonAsync(string numeroDestino, string nombrePlantilla, string idioma, Dictionary<string, string> parametros, string urlBoton)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                Console.WriteLine("Configuracion de WhatsApp incompleta");
                UltimoError = "Configuracion de WhatsApp incompleta";
                return false;
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new { code = idioma },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = parametros.Select(p => new
                            {
                                type = "text",
                                text = p.Value
                            }).ToList()
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

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Plantilla con boton '{nombrePlantilla}' enviada a {numeroDestino}");
                UltimoError = null;
                return true;
            }
            else
            {
                Console.WriteLine($"Error al enviar plantilla con boton: {responseContent}");
                UltimoError = responseContent;
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepcion al enviar plantilla con boton: {ex.Message}");
            UltimoError = ex.Message;
            return false;
        }
    }

    public async Task<bool> EnviarMensajeConImagenAsync(string numeroDestino, string mensaje, string imagenUrl)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                Console.WriteLine("Configuracion de WhatsApp incompleta");
                UltimoError = "Configuracion de WhatsApp incompleta";
                return false;
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "image",
                image = new
                {
                    link = imagenUrl
                },
                text = new
                {
                    body = mensaje
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Mensaje con imagen enviado a {numeroDestino}");
                UltimoError = null;
                return true;
            }
            else
            {
                Console.WriteLine($"Error: {responseContent}");
                UltimoError = responseContent;
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepcion: {ex.Message}");
            UltimoError = ex.Message;
            return false;
        }
    }

    public async Task<bool> EnviarPlantillaConImagenAsync(string numeroDestino, string nombrePlantilla, string idioma, Dictionary<string, string> parametros, string imagenUrl)
    {
        try
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                Console.WriteLine("Configuracion de WhatsApp incompleta");
                UltimoError = "Configuracion de WhatsApp incompleta";
                return false;
            }

            var url = $"https://graph.facebook.com/{_apiVersion}/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "template",
                template = new
                {
                    name = nombrePlantilla,
                    language = new { code = idioma },
                    components = new object[]
                    {
                        new
                        {
                            type = "header",
                            parameters = new object[]
                            {
                                new
                                {
                                    type = "image",
                                    image = new
                                    {
                                        link = imagenUrl
                                    }
                                }
                            }
                        },
                        new
                        {
                            type = "body",
                            parameters = parametros.Select(p => new
                            {
                                type = "text",
                                text = p.Value
                            }).ToList()
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Plantilla con imagen '{nombrePlantilla}' enviada a {numeroDestino}");
                UltimoError = null;
                return true;
            }
            else
            {
                Console.WriteLine($"Error al enviar plantilla con imagen: {responseContent}");
                UltimoError = responseContent;
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepcion al enviar plantilla con imagen: {ex.Message}");
            UltimoError = ex.Message;
            return false;
        }
    }
}