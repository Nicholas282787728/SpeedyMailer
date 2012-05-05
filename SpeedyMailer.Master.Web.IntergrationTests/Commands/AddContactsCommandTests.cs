using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
{
    [TestFixture]
    public class AddContactsCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenAListOfContactsAndAList_ShouldAddThemToTheStore()
        {

            const string listName = "MyList";
            var listId = UI.ExecuteCommand<CreateListCommand,string>(x =>
                                                                         {
                                                                             x.Name = listName;
                                                                         });
            var contacts = Fixture.CreateMany<Contact>(10).ToList();

            UI.ExecuteCommand<AddContactsCommand,long>(x=>
                                                      {
                                                          x.Contacts = contacts;
                                                          x.ListId = listId;

                                                      });

            var result = Query<Contact>(x => x.MemberOf.Any(list=> list == listId));

            var resultNames = result.Select(x => x.Name).ToList();
            var names = contacts.Select(x => x.Name).ToList();

            resultNames.Should().BeEquivalentTo(names);
        }

        [Test]
        public void Execute_WhenWeHaveDuplicates_ShouldRemovetheDuplicates()
        {
            const string listName = "MyList";
            var listId = UI.ExecuteCommand<CreateListCommand, string>(x =>
            {
                x.Name = listName;
            });

            var contacts = Fixture.CreateMany<Contact>(10).ToList();
            var theDuplicate = contacts[9];

            contacts.Add(theDuplicate);

            UI.ExecuteCommand<AddContactsCommand, long>(x =>
            {
                x.Contacts = contacts;
                x.ListId = listId;

            });

            var result = Query<Contact>(x => x.MemberOf.Any(list => list == listId));

            result.Should().HaveCount(10);
            result.Count(x => x.Name == theDuplicate.Name).Should().Be(1);

        }
    }
}
