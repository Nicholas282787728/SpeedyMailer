﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
    public class CreativesController : ApiController
    {
	    private SpeedyMailer.Core.Apis.Api _api;

	    public CreativesController(SpeedyMailer.Core.Apis.Api api)
	    {
		    _api = api;
	    }

	    [POST("/creatives/creative"),HttpPost]
		public void SaveCreative(CreativeModel creativeModel)
	    {
		    _api.Call<ServiceEndpoints.SaveCreative>(x =>
			                                             {
				                                             x.Body = creativeModel.Body;
				                                             x.DealUrl = creativeModel.DealUrl;
				                                             x.Subject = creativeModel.Subject;
				                                             x.ListId = creativeModel.ListId;
			                                             });
	    }
    }

	public class CreativeModel
	{
		public string Body { get; set; }
		public string DealUrl { get; set; }
		public string Subject { get; set; }
		public string ListId { get; set; }
	}
}
