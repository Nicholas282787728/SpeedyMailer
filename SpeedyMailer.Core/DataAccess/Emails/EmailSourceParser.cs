using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace SpeedyMailer.Core.DataAccess.Emails
{
    public class EmailSourceParser : IEmailSourceParser
    {
        public List<string> Deals(string emailSource)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(emailSource);
            var extractedDealList = doc.DocumentNode.SelectNodes("//a[@href]");
            if (extractedDealList != null)
            {
                return Enumerable.Distinct<string>(extractedDealList.Select(link => link.GetAttributeValue("href", ""))).ToList();

            }
            return new List<string>();
        }
    }
}