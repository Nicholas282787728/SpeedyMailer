using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.EmailPool.MailDrone.Mail
{
    public class MailParser:IMailParser
    {
        private readonly IEmailSourceWeaver weaver;
        private MailParserInitializer mailParserInitializer;

        public MailParser(IEmailSourceWeaver weaver)
        {
            this.weaver = weaver;
        }

        public string Parse(ExtendedRecipient recipient)
        {
            var body = weaver.WeaveDeals(mailParserInitializer.Body, recipient.DealUrl);

            return body;
        }

        public void Initialize(MailParserInitializer mailParserInitializer)
        {
            this.mailParserInitializer = mailParserInitializer;
        }
    }
}