﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchDeliveryClassificationHeuristicsTaskTests : IntegrationTestBase
	{
		public FetchDeliveryClassificationHeuristicsTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenStarted_ShouldFetchTheHeuristics()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Heuristics.GetDeliveryRules,
				DeliverabilityClassificationRules>(x =>
													   {
														   x.Rules = new List<HeuristicRule>
							                                             {
								                                             new HeuristicRule {Condition = "hard bounce rule", Type = Classification.HardBounce},
								                                             new HeuristicRule {Condition = "blocking rule", Type = Classification.TempBlock, Data = new HeuristicData {TimeSpan = TimeSpan.FromHours(4)}},
							                                             };
													   });

			var task = new FetchDeliveryClassificationHeuristicsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<DeliverabilityClassificationRules>(1000);

			var result = DroneActions.FindSingle<DeliverabilityClassificationRules>();

			result.Rules.Should().Contain(x => x.Condition == "hard bounce rule");
			result.Rules.Should().Contain(x => x.Condition == "blocking rule");
		}
	}
}
