using Raven.Client;
using Rhino.Mocks;

namespace SpeedyMailer.Tests.Core.Unit.Database
{
    public static class DocumentStoreFactory
    {
        public static IDocumentStore StubDocumentStoreWithSession(IDocumentSession session)
        {
            var store = MockRepository.GenerateStub<IDocumentStore>();


            store.Stub(x => x.OpenSession()).Return(session);
            return store;
        }

        public static IDocumentStore StubDocumentStoreWithStubSession()
        {
            var session = MockRepository.GenerateStub<IDocumentSession>();

            return StubDocumentStoreWithSession(session);
        }
    }
}