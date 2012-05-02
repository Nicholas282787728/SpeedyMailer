using System.IO;
using NUnit.Framework;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Tests.Acceptance.Specs.Drone
{
    [TestFixture]
    public class SendingEmail : IntegrationTestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SendCreative_ShouldStartTheSendingTaskOnTheDrone()
        {
            AddList();
            AddContacts();
        }

        private void AddList()
        {
            Master.ExecuteCommand<CreateListCommand, string>(x =>
                                                                 {
                                                                     x.Name = "Default list";
                                                                     x.Id = 1;
                                                                 });
        }

        private void AddContacts()
        {
            var stream = File.OpenRead("fixture/contacts/csv-sample.csv");
            Master.ExecuteCommand<ParseCsvFileCommand, UploadListCommandResult>(x =>
                                                                                 {
                                                                                     x.ListId = "lists/1";
                                                                                     x.Source = stream;
                                                                                 });
        }
    }
}
