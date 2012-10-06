using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Service.Commands
{
    public class AddCreativeCommand:Command<string>
    {
        private readonly IDocumentStore _store;

        public AddCreativeCommand(IDocumentStore store)
        {
            _store = store;
        }

        public override string Execute()
        {
            using (var session = _store.OpenSession())
            {
                var creative = new Creative
                                   {
                                       Body = Body,
                                       Lists = Lists.ToList(),
                                       Subject = Subject,
                                       UnsubscribeTemplateId = UnsubscribeTemplateId,
									   DealUrl = DealUrl
                                   };
                session.Store(creative);
                session.SaveChanges();

                return creative.Id;
            }
        }

        public string Body { get; set; }
        public string Subject { get; set; }
        public IList<string> Lists { get; set; }
        public string UnsubscribeTemplateId { get; set; }
	    public string DealUrl { get; set; }
    }
}