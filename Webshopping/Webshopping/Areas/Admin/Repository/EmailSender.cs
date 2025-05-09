using System.Net.Mail;
using System.Net;

namespace Webshopping.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("huuthong1211@gmail.com", "sdnvpaorqaewyybo")
            };

            return client.SendMailAsync(
                new MailMessage(from: "huuthong1211@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
