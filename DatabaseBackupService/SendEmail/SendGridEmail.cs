using FluentEmail.Core;
using FluentEmail.SendGrid;
using System.Reflection;

namespace SendEmail
{
    public class EmailReceiver
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }
    }

    public class SendGridEmail : ISendEmail
    {
        public async Task<bool> SendMessage(string subject, string body)
        {
            IConfiguration Config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            FluentEmail.Core.Models.SendResponse email = new();

            string EmailFrom = Config.GetSection("EmailConfig")["EmailFrom"];
            IEnumerable<EmailReceiver> EmailReceivers = Config.GetSection("EmailConfig").GetSection("EmailReceivers").Get<IEnumerable<EmailReceiver>>();
            List<FluentEmail.Core.Models.Address> emailAddresses = new List<FluentEmail.Core.Models.Address>(); ;
            foreach (var emailReceiver in EmailReceivers)
            {
                string emailAddress = emailReceiver.EmailAddress;
                string name = emailReceiver.Name;
                FluentEmail.Core.Models.Address fEmailAddress = new FluentEmail.Core.Models.Address(emailAddress, name);
                emailAddresses.Add(fEmailAddress);
            }

            string SendgridApiKey = Config.GetSection("EmailConfig")["SendgridApiKey"];

            Email.DefaultSender = new SendGridSender(SendgridApiKey);

            if (EmailReceivers.Count() > 0)
            {
                email = await new Email()
                    .Tag(Guid.NewGuid().ToString())
                    .SetFrom(EmailFrom)
                    .To(emailAddresses)
                    .Subject(subject)
                    .Body(body, true)
                    .SendAsync();
            }
            if (email.ErrorMessages.Count == 0)
                return email.Successful;
            else
                throw new Exception(email.ErrorMessages.ToString());

        }
    }
}

