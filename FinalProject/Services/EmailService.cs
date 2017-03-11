using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Service;
using Microsoft.AspNet.Identity;
using static FinalProject.Shared.Constants;

namespace FinalProject.Services
{
    public interface IEmailService : IIdentityMessageService, IService
    {
        Task SendForStudentAsync(StudentDataTransfer destination, string subject, string content);
    }

    public class EmailService : IEmailService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            var appSettings = WebConfigurationManager.AppSettings;
            var credentials = new NetworkCredential(
                appSettings[EmailServerLoginConfigName],
                appSettings[EmailServerPasswordConfigName]);

            var emailClient = new SmtpClient(
                appSettings[EmailServerHostNameConfigName],
                int.Parse(appSettings[EmailServerHostPortConfigName]))
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = credentials,
                EnableSsl = true
            };

            var mailMessage = new MailMessage(credentials.UserName, message.Destination)
            {
                Body = message.Body,
                Subject = message.Subject,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8
            };

            mailMessage.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(message.Body, Encoding.UTF8, MediaTypeNames.Text.Plain));

            mailMessage.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(message.Body, Encoding.UTF8, MediaTypeNames.Text.Html));

            await emailClient.SendMailAsync(mailMessage);
        }

        public async Task SendForStudentAsync(StudentDataTransfer destination, string subject, string content)
        {
            var body = string.Format(Resource.StudentDefaultEmailContent, destination.FirstName, content);
            var message = new IdentityMessage {Subject = subject, Destination = destination.Email, Body = body};

            await SendAsync(message);
        }
    }

#if DEBUG
    internal class FakeEmailService : IEmailService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.CompletedTask;
        }

        public Task SendForStudentAsync(StudentDataTransfer destination, string subject, string content)
        {
            return Task.CompletedTask;
        }
    }
#endif
}