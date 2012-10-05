using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.Mail;
using Quartz;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Master;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Commands;
using Antlr4.StringTemplate;
using Template = Antlr4.StringTemplate.Template;


namespace SpeedyMailer.Drones.Tasks
{
	public class FetchCreativeFragmentsTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInSeconds(10).RepeatForever());
		}

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly Api _api;
			private readonly SendCreativePackageCommand _sendCreativePackageCommand;
			private readonly ICreativeBodySourceWeaver _creativeBodySourceWeaver;
			private readonly UrlBuilder _urlBuilder;

			public Job(Api api, SendCreativePackageCommand sendCreativePackageCommand,
					   ICreativeBodySourceWeaver creativeBodySourceWeaver, UrlBuilder urlBuilder)
			{
				_urlBuilder = urlBuilder;
				_creativeBodySourceWeaver = creativeBodySourceWeaver;
				_sendCreativePackageCommand = sendCreativePackageCommand;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
			    var creativeFragment = _api
			        .Call<ServiceEndpoints.FetchFragment, CreativeFragment>();

                if (creativeFragment == null)
                    return;

				var recipiens = creativeFragment.Recipients;
				Trace.WriteLine("receipents in the fragment: " + recipiens.Count);

				foreach (var recipient in recipiens)
				{
					var package = ToPackage(recipient, creativeFragment);

					_sendCreativePackageCommand.Package = package;
					_sendCreativePackageCommand.Execute();
				}
			}

			private CreativePackage ToPackage(Contact recipient, CreativeFragment creativeFragment)
			{
				return new CreativePackage
						{
							Subject = creativeFragment.Subject,
							Body = PersonalizeBody(creativeFragment, recipient),
							To = recipient.Email
						};
			}

			private string ServiceEndpoint(Service service,Func<Service,string> endpointSelector)
			{
				return string.Format("{0}/{1}", service.BaseUrl, endpointSelector(service));
			}

			private string PersonalizeBody(CreativeFragment fragment, Contact contact)
			{
				var service = fragment.Service;

				var dealUrl = _urlBuilder
					.Base(ServiceEndpoint(service,x=> x.DealsEndpoint))
					.AddObject(GetDealUrlData(fragment, contact))
					.AppendAsSlashes();
				
				var unsubsribeUrl = _urlBuilder
					.Base(ServiceEndpoint(service,x=> x.UnsubscribeEndpoint))
					.AddObject(GetDealUrlData(fragment, contact))
					.AppendAsSlashes();

				var deal =  _creativeBodySourceWeaver.WeaveDeals(fragment.Body, dealUrl);

				var unsubscribeTemplate = new Template(fragment.UnsubscribeTemplate);
				unsubscribeTemplate.Add("url", unsubsribeUrl);

				return deal + unsubscribeTemplate.Render();
			}

			private static DealUrlData GetDealUrlData(CreativeFragment fragment, Contact contact)
			{
				return new DealUrlData
						{
							CreativeId = fragment.CreativeId,
							ContactId = contact.Id
						};
			}
		}
	}
}