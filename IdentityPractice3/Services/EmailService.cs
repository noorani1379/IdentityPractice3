using System.Net.Mail;
using System.Net;
using System.Text;

namespace IdentityPractice3.Services
{
    public class EmailService
    {
        public Task Execute(string UserEmail, string Body, string Subject)
        {
            //enable less secure apps in account google with link
            //https://myaccount.google.com/lesssecureapps

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 1000000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("fortestmyprojects@gmail.com", "Aa@123456");
            MailMessage message = new MailMessage("fortestmyprojects@gmail.com", UserEmail, Subject, Body);
            message.IsBodyHtml = true;
            message.BodyEncoding = UTF8Encoding.UTF8;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
           // client.Send(message);
            return Task.CompletedTask;
        }
    }
}
