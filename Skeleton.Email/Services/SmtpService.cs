using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Skeleton.Email.Interfaces;

namespace Skeleton.Email.Services
{
    public class SmtpService(ILogger<SmtpService> logger, IOptions<SmtpOptions> smtpOptionsAccessor) : IMailService
    {
        private readonly ILogger<SmtpService> _logger = logger;
        private readonly SmtpOptions _smtpOptions = smtpOptionsAccessor.Value;

        public async Task<bool> SendAsync(MimeMessage message)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_smtpOptions.FolderPath))
                {
                    message.WriteTo(Path.Combine(_smtpOptions.FolderPath, $"{Path.GetRandomFileName()}.eml"));
                }
                else
                {
                    if (!_smtpOptions.IsProduction)
                    {
                        message.Subject = $"TEST SYSTEM EMAIL - {message.Subject} - TEST SYSTEM EMAIL";
                    }

                    using SmtpClient client = new();
                    await client.ConnectAsync(_smtpOptions.ServerAddress, _smtpOptions.Port, _smtpOptions.IsSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                string tempFileName = Path.GetRandomFileName();
                _logger.LogError(ex, "Unable to send email, writing {FileName} to failed folder", tempFileName);
                message.WriteTo(Path.Combine(_smtpOptions.FailedSendFolderPath, $"{tempFileName}.eml"));

                return false;
            }
        }
    }
}
