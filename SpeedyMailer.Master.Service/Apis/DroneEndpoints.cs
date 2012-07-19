using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Service.Apis
{
	public class DroneEndpoints
	{
		public class Manage
		{
			public class Wakeup : ApiCall
			{
				public Wakeup()
					: base("/manage/wakeup")
				{
					CallMethod = RestMethod.Post; 
				}

				public class Response
				{
					public DroneStatus DroneStatus { get; set; }
				}
			}
		}
	}
}