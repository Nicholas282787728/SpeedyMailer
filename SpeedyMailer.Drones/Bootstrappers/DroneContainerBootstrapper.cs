﻿using Nancy.Bootstrapper;
using Ninject;
using Ninject.Modules;
using Quartz;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Drones.Bootstrappers
{
	public static class DroneContainerBootstrapper
	{
		public static IKernel Kernel { get; private set; }

		static DroneContainerBootstrapper()
		{
			Kernel = ApplyBindLogic(ContainerBootstrapper.Bootstrap());
		}

		public static IKernel ApplyBindLogic(AssemblyGatherer assemblyGatherer)
		{
			return assemblyGatherer.Analyze(x => x.AssembiesContaining(new[]
			                                                           	{
			                                                           		typeof (DroneAssemblyMarker),
			                                                           		typeof (ISchedulerFactory)
			                                                           	}))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();
		}
	}

	public class NancyModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<INancyBootstrapper>().ToProvider(new NancyBootstrapperProvider(
			                                             	kernel =>
			                                             	DroneContainerBootstrapper
			                                             		.ApplyBindLogic(ContainerBootstrapper.Bootstrap(kernel)))
				);
		}
	}
}