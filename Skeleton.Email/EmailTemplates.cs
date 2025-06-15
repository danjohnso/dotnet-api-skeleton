using MimeKit;
using MimeKit.Text;

namespace Skeleton.Email
{
    public static class EmailTemplates
    {
        internal const string TemplateNamespace = "Skeleton.Email.Templates";

        public static class SampleEmail
        {
            private static readonly string ResourceName = $"{TemplateNamespace}.SampleEmail.html";

            public static MimeMessage Build(string toEmailAddress)
            {
                string messageBody;
                using (Stream stream = typeof(EmailTemplates).Assembly.GetManifestResourceStream(ResourceName)
                            ?? throw new NullReferenceException($"Unable to load email template '{ResourceName}'"))
                {
                    using StreamReader sr = new(stream ?? throw new InvalidOperationException());
                    messageBody = sr.ReadToEnd();
                }

                MimeMessage message = new()
                {
                    Subject = "Sample Email"
                };

                message.To.Add(MailboxAddress.Parse(toEmailAddress));

                //can just do a string replace on tokens for whatever dynamic info is in the email template
                //messageBody
                //.Replace("{{ENVIRONMENT_URL}}", EnvironmentUrl)
                //.Replace("{{TITLE}}", Title)
                //.Replace("{{MESSAGE}}", Message)

                message.Body = new TextPart(TextFormat.Html) { Text = messageBody };

                return message;
            }
        }
    }
}
