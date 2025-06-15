using MimeKit;

namespace Skeleton.Email.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendAsync(MimeMessage message);
    }
}
