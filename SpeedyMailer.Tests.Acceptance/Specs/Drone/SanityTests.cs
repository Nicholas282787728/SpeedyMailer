using NUnit.Framework;
using SpeedyMailer.Drone;
using SpeedyMailer.Master.Web.UI;

namespace SpeedyMailer.Tests.Acceptance.Specs.Drone
{
    [TestFixture]
    public class SanityTests : DroneIntegrationTestBase
    {
        [Test]
        public void Sanity_ShouldSendTheFirstBanchOfEmailsWhenActivated()
        {
            DroneHost.Main(new string[] {});
        }
    }
}