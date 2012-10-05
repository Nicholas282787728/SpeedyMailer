using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
	[TestFixture]
	public class AddCreativeCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenAllCreateParametersAreValid_ShouldAddCreativeToStore()
        {
	        var creativeId = UIActions.ExecuteCommand<AddCreativeCommand, string>(x =>
                                                                                   {
                                                                                       x.Subject = "Subject";
                                                                                       x.Body = "Body";
                                                                                       x.Lists = new List<string>
	                                                                                                 {
		                                                                                                 "list1",
		                                                                                                 "list2"
	                                                                                                 };
	                                                                                   x.DealUrl = "dealUrl";
                                                                                   });

            var result = Load<Creative>(creativeId);

            result.Body.Should().Be("Body");
            result.Subject.Should().Be("Subject");
            result.Lists.Should().Contain(new List<string> { "list1","list2" });
			result.DealUrl.Should().Be("dealUrl");
        }
	}
}
