using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using RamsCottons.Data;

namespace RamsCottons.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Envía el link para confirmar la cuenta nueva
        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            await SendEmailAsync(email, "Confirma tu cuenta - Rams Cottons",
                $"<div style='font-family: Arial; border: 1px solid #eee; padding: 20px; border-radius: 10px; max-width: 600px; margin: 0 auto;'>" +
                $"<h2 style='color: #ff6600;'>¡Bienvenido a Rams Cottons!</h2>" +
                $"<p>Hola <strong>{user.NombreCompleto}</strong>,</p>" +
                $"<p>Para activar tu cuenta, haz clic en el botón de abajo:</p>" +
                $"<a href='{confirmationLink}' style='background: #ff6600; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;'>Confirmar mi cuenta</a>" +
                $"<p style='color: #888; font-size: 12px; margin-top: 20px;'>Si no solicitaste esta cuenta, puedes ignorar este correo.</p>" +
                $"</div>");

        // Envía el link para recuperar la contraseña
        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            await SendEmailAsync(email, "Restablecer contraseña - Rams Cottons",
                $"<div style='font-family: Arial; border: 1px solid #eee; padding: 20px; border-radius: 10px; max-width: 600px; margin: 0 auto;'>" +
                $"<h2 style='color: #ff6600;'>Recuperación de contraseña</h2>" +
                $"<p>Recibimos una solicitud para cambiar tu contraseña de Rams Cottons.</p>" +
                $"<p>Haz clic en el enlace para continuar:</p>" +
                $"<a href='{resetLink}' style='background: #ff6600; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;'>Cambiar contraseña</a>" +
                $"<p style='color: #888; font-size: 12px; margin-top: 20px;'>Si no solicitaste este cambio, puedes ignorar este correo.</p>" +
                $"</div>");

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            await SendEmailAsync(email, "Código de recuperación - Rams Cottons",
                $"<div style='font-family: Arial; border: 1px solid #eee; padding: 20px; border-radius: 10px; max-width: 600px; margin: 0 auto;'>" +
                $"<h2 style='color: #ff6600;'>Código de recuperación</h2>" +
                $"<p>Tu código de recuperación es:</p>" +
                $"<h1 style='color: #ff6600; font-size: 32px; letter-spacing: 4px;'>{resetCode}</h1>" +
                $"<p style='color: #888; font-size: 12px; margin-top: 20px;'>Si no solicitaste este código, puedes ignorar este correo.</p>" +
                $"</div>");

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var host = _config["EmailSettings:Host"];
                var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
                var userName = _config["EmailSettings:UserName"];
                var password = _config["EmailSettings:Password"];
                var fromEmail = _config["EmailSettings:FromEmail"];
                var fromName = _config["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("Configuración de EmailSettings incompleta en appsettings.json");
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName ?? "Rams Cottons", fromEmail ?? userName));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlMessage };

                using var client = new SmtpClient();

                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(userName, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Correo enviado exitosamente a {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enviando correo a {email}: {ex.Message}");
                throw; // ← Propagar el error para que el usuario lo vea
            }
        }
    }
}