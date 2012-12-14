﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class HeuristicsController : ApiController
	{
		private readonly SpeedyMailer.Core.Apis.Api _api;

		public HeuristicsController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[POST("heuristics/delivery")]
		public void SetDelivery(DeliverabilityClassificationRulesModel model)
		{
			_api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.DeliverabilityClassificationRules = ToHeuristicsRules(model));
		}

		private DeliverabilityClassificationRules ToHeuristicsRules(DeliverabilityClassificationRulesModel model)
		{
			return new DeliverabilityClassificationRules
				{
					HardBounceRules = model.HardBounceRules,
					BlockingRules = model.BlockingRules.Select(x => new HeuristicRule { Condition = x.Condition, TimeSpan = TimeSpan.FromHours(x.TimeSpan) }).ToList()
				};
		}

		[GET("heuristics/delivery")]
		public IList<DeliverabilityClassificationRulesModel> GetDelivery()
		{
			var result = _api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, DeliverabilityClassificationRules>();
			return new[] { ToHeuristicsModel(result) };
		}

		private DeliverabilityClassificationRulesModel ToHeuristicsModel(DeliverabilityClassificationRules result)
		{
			return new DeliverabilityClassificationRulesModel
				{
					HardBounceRules = result.HardBounceRules,
					BlockingRules = result.BlockingRules.Select(x => new BlockingRulesModel { Condition = x.Condition, TimeSpan = x.TimeSpan.TotalHours }).ToList()
				};
		}
	}

	public class BlockingRulesModel
	{
		public string Condition { get; set; }
		public double TimeSpan { get; set; }
	}

	public class DeliverabilityClassificationRulesModel
	{
		public List<string> HardBounceRules { get; set; }
		public List<BlockingRulesModel> BlockingRules { get; set; }
	}
}
