﻿using System;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Service.Container;
using Ninject;

namespace SpeedyMailer.Master.Service
{
    public class ServiceHost
    {
    	public static void Main(string[] args)
    	{
    		var kernel = ServiceContainerBootstrapper.Kernel;
    		var service = kernel.Get<Service>();

    		service.Start();
			Console.WriteLine("To stop press any key");
    		Console.ReadKey();
			service.Stop();
    	}
    }

	public class Service
	{
		private readonly NancyHost _nancyHost;

		public Service(INancyBootstrapper ninjectNancyBootstrapper)
		{
			_nancyHost = new NancyHost(ninjectNancyBootstrapper, new Uri("http://localhost:2589/"));
		}

		public void Start()
    	{
    		_nancyHost.Start();
    		Console.WriteLine("Nancy now listening - navigating to http://localhost:2589. Press enter to stop");
    	}

    	public void Stop()
    	{
    		_nancyHost.Stop();
    		Console.WriteLine("Stopped. Good bye!");
    	}
	}
}