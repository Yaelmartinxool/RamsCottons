using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using RamsCottons.Data;

namespace RamsCottons.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        // Envía el link para confirmar la cuenta nueva
        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            await SendEmailAsync(email, "Confirma tu cuenta - Rams Cottons", 
                $"<div style='font-family: Arial; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>" +
                $"<h2 style='color: #ff6600;'>¡Bienvenido a Rams Cottons!</h2>" +
                $"<p>Para activar tu cuenta, haz clic en el botón de abajo:</p>" +
                $"<a href='{confirmationLink}' style='background: #ff6600; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Confirmar mi cuenta</a>" +
                $"</div>");

        // Envía el link para recuperar la contraseña
        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            await SendEmailAsync(email, "Restablecer contraseña - Rams Cottons", 
                $"<div style='font-family: Arial; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>" +
                $"<h2 style='color: #ff6600;'>Recuperación de contraseña</h2>" +
                $"<p>Recibimos una solicitud para cambiar tu contraseña de Rams Cottons.</p>" +
                $"<p>Haz clic en el enlace para continuar:</p>" +
                $"<a href='{resetLink}' style='background: #ff6600; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Cambiar contraseña</a>" +
                $"<p style='color: #888; font-size: 12px; margin-top: 20px;'>Si no solicitaste este cambio, puedes ignorar este correo.</p>" +
                $"</div>");

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            await SendEmailAsync(email, "Código de recuperación", $"Tu código es: {resetCode}");

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rams Cottons", "ymartinxool@gmail.com")); 
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            try 
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                
                // Usando tus credenciales proporcionadas
                await client.AuthenticateAsync("ymartinxool@gmail.com", "nndfggtwlgfnrscm");
                
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Esto aparecerá en la consola de "Output" o "Depuración" de Visual Studio si falla
                Console.WriteLine($"DEBUG: Error enviando email: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}