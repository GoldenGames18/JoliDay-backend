using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using JoliDay.ViewModel;

namespace JoliDay.Services
{
    public class ServiceEmail : IServiceEmail
    {
        private readonly IConfiguration _config;
        public ServiceEmail(IConfiguration configuration) 
        {
            this._config = configuration;
        }

        public bool SendEmail(ContactMeViewModel contactMe)
        {
            try 
            {
                using (ISmtpClient smtp = new SmtpClient()) 
                {
                    smtp.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_config["Email:AdminOne"], _config["Email:Password"]);
                    smtp.Send(GenerateEmail(contactMe));
                    smtp.Disconnect(true);
                }
                return true;
            }catch (Exception)
            {
                return false;
            }


        }

        private MimeMessage GenerateEmail(ContactMeViewModel contactMe) 
        {

            MimeMessage createEmail = new MimeMessage();
            createEmail.From.Add(MailboxAddress.Parse(_config["Email:AdminOne"]));
            createEmail.To.Add(MailboxAddress.Parse(contactMe.Email));
            createEmail.To.Add(MailboxAddress.Parse(_config["Email:AdminTwo"]));
            createEmail.To.Add(MailboxAddress.Parse(_config["Email:AdminOne"]));
            createEmail.Subject = contactMe.Subject;
            createEmail.Body = new TextPart(TextFormat.Html) { Text = contactMe.Body};
            return createEmail;
        }
    }
}
