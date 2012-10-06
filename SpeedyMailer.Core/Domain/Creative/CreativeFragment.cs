using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Master;

namespace SpeedyMailer.Core.Domain.Creative
{
	public class CreativeFragment
	{
		public string Id { get; set; }
		public string UnsubscribeTemplate { get; set; }
		public List<Recipient> Recipients { get; set; }
		public string Body { get; set; }
		public string Subject { get; set; }
		public Service Service { get; set; }
		public string CreativeId { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FragmentStatus Status { get; set; }
	}

	public class Recipient
	{
		public string ContactId { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public int Interval { get; set; }
	}
}