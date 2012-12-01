﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Modules
{
	public class DronesModule : NancyModule
	{
		private readonly IDocumentStore _documentStore;

		public DronesModule(IDocumentStore documentStore)
			: base("/drones")
		{
			_documentStore = documentStore;

			Post["/"] = x =>
				{
					var model = this.Bind<ServiceEndpoints.Drones.RegisterDrone>();
					using (var session = _documentStore.OpenSession())
					{
						session.Store(new Drone
							{
								BaseUrl = model.BaseUrl,
								Id = model.Identifier,
								LastUpdated = DateTime.Parse(model.LastUpdate),
								Domain = model.Domain
							});
						session.SaveChanges();
					}
					return Response.AsText("OK");
				};

			Get["/dnsbl"] = x =>
								{
									using (var sr = new StreamReader("data/dnsbl.js"))
									{
										var data = sr.ReadToEnd();
										return Response.AsJson(JsonConvert.DeserializeObject<List<Dnsbl>>(data));
									}
								};

			Post["/state-snapshot"] = x =>
				                          {
					                          var model = this.Bind<ServiceEndpoints.Drones.SendStateSnapshot>();

											  using (var session = documentStore.OpenSession())
											  {
												  session.Store(new DroneStateSnapshoot
													                {
														                Drone = model.Drone,
																		RawLogs = model.RawLogs,
																		MailBounced = model.MailBounced,
																		MailSent = model.MailSent,
																		MailDeferred = model.MailDeferred,
																		ClickActions = model.ClickActions,
																		UnsubscribeRequests = model.UnsubscribeRequests
													                });

												  session.SaveChanges();
											  }

											  return Response.AsText("OK");
										  };
		}
	}
}
