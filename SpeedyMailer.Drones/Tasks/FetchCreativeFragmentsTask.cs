using System.Collections.Generic;
using System.Linq;
using NLog;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;


namespace SpeedyMailer.Drones.Tasks
{
	public class FetchCreativeFragmentsTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(1).RepeatForever());
		}

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly Api _api;
			private readonly Framework _framework;
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly OmniRecordManager _omniRecordManager;
			private readonly MapToCreativePackageCommand _mapToCreativePackageCommand;

			public Job(Framework framework,
					   Api api,
					   CreativePackagesStore creativePackagesStore,
					   OmniRecordManager omniRecordManager,
					   MapToCreativePackageCommand mapToCreativePackageCommand)
			{
				_mapToCreativePackageCommand = mapToCreativePackageCommand;
				_omniRecordManager = omniRecordManager;
				_framework = framework;
				_creativePackagesStore = creativePackagesStore;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var groupsSendingPolicies = _omniRecordManager.GetSingle<GroupsSendingPolicies>() ?? new GroupsSendingPolicies();

				if (_creativePackagesStore.AreThereAnyPackages())
				{
					var packages = _creativePackagesStore.GetAll();

					if (!context.Scheduler.IsJobsRunning<SendCreativePackagesWithIntervalTask>() && AreThereActiveGroups(packages, groupsSendingPolicies))
					{
						StartGroupSendingJobs(packages, groupsSendingPolicies);

						return;
					}
				}

				var creativeFragment = _api
					.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>();

				if (creativeFragment == null)
					return;

				SaveCurrentCreativeFragment(creativeFragment);

				var recipiens = creativeFragment.Recipients;
				var creativePackages = recipiens.Select(x => ToCreativePackage(creativeFragment, x)).ToList();

				_creativePackagesStore.BatchInsert(creativePackages);

				StartGroupSendingJobs(creativePackages, groupsSendingPolicies);
			}

			private void SaveCurrentCreativeFragment(CreativeFragment creativeFragment)
			{
				_omniRecordManager.UpdateOrInsert(
					new CurrentExecutingCreativeFragment
						{
							Id = "CurrentExecutingCreativeFragment",
							Body = creativeFragment.Body,
							CreativeId = creativeFragment.CreativeId,
							FromName = creativeFragment.FromName,
							FromAddressDomainPrefix = creativeFragment.FromAddressDomainPrefix,
							Service = creativeFragment.Service,
							Subject = creativeFragment.Subject,
							UnsubscribeTemplate = creativeFragment.UnsubscribeTemplate
						}
					);
			}

			private CreativePackage ToCreativePackage(CreativeFragment creativeFragment, Recipient x)
			{
				_mapToCreativePackageCommand.CreativeFragment = creativeFragment;
				_mapToCreativePackageCommand.Recipient = x;
				return _mapToCreativePackageCommand.Execute();
			}

			private bool AreThereActiveGroups(IEnumerable<CreativePackage> packages, GroupsSendingPolicies groupsSendingPolicies)
			{
				return GetActiveGroups(packages, groupsSendingPolicies).Any();
			}

			private void StartGroupSendingJobs(IEnumerable<CreativePackage> recipiens, GroupsSendingPolicies sendingPolicies)
			{
				var groups = GetActiveGroups(recipiens, sendingPolicies);

				foreach (var group in groups)
				{
					_framework.StartTasks(new SendCreativePackagesWithIntervalTask(x =>
																					   {
																						   x.Group = @group.Group;
																					   },
																				   x => x.WithIntervalInSeconds(@group.Interval).WithRepeatCount(@group.Count)
											  ));
				}
			}

			private static IEnumerable<PackageInfo> GetActiveGroups(IEnumerable<CreativePackage> recipiens, GroupsSendingPolicies sendingPolicies)
			{
				return recipiens
					.GroupBy(x => x.Group)
					.Select(x => new PackageInfo { Group = x.Key, Interval = x.First().Interval, Count = x.Count() })
					.Where(x => !sendingPolicies
									 .GroupSendingPolicies
									 .EmptyIfNull()
									 .ContainsKey(x.Group))
					.ToList();
			}
		}
	}
}