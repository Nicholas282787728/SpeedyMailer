using NUnit.Framework;
using Quartz;
using Rhino.Mocks;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Drone
{
    [TestFixture]
    public class DroneTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Start_ShouldStartTheDroneJob()
        {
            //Arrange
            var schedular = MockRepository.GenerateStub<IScheduler>();

            var builder = new MockedDroneBuilder();
            Drone drone = builder.Build();
            //Act

            //Assert
        }
    }
}