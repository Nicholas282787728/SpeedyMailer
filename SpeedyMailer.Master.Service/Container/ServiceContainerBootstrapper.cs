using Nancy.Bootstrapper;
using Ninject;
using Ninject.Modules;
using Quartz;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Master.Service.Container
{
    public static class ServiceContainerBootstrapper
    {
        public static IKernel Kernel { get; private set; }

        static ServiceContainerBootstrapper()
        {
            Kernel = ApplyBindLogic(ContainerBootstrapper.Bootstrap());
        }

        public static IKernel ApplyBindLogic(AssemblyGatherer assemblyGatherer)
        {
            return assemblyGatherer.Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (CoreAssemblyMarker),
                                                            typeof (ServiceAssemblyMarker),
                                                            typeof (IRestClient),
                                                            typeof (ISchedulerFactory)
                                                        }))
                .BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
                .Storage<IDocumentStore>(x=> x.Provider<RavenDocumentStoreProvider>())
                .Settings(x => x.UseDocumentDatabase())
                .Done();
        }
    }

    public class NancyFxModule : NinjectModule
    {
        public override void Load()
        {
	        var scheduler = Kernel.Get<IScheduler>();
            Kernel.Bind<INancyBootstrapper>().ToProvider(new NancyBootstrapperProvider(
                                                            kernel =>
                                                            ServiceContainerBootstrapper
	                                                            .ApplyBindLogic(ContainerBootstrapper.Bootstrap(kernel)), scheduler)
                );
        }
    }
}
