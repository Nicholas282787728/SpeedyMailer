using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class AnalyzePostfixLogsTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(10).RepeatForever());
		}

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly EventDispatcher _eventDispatcher;
			private readonly ParseLogsCommand _parseLogsCommand;
			private readonly LogsStore _logsStore;
			private readonly OmniRecordManager _omniRecordManager;
			private readonly IntervalRulesStore _intervalRulesStore;
			private readonly CreativeFragmentSettings _creativeFragmentSettings;
			private readonly Logger _logger;
			private readonly ParseCreativeIdFromLogsCommand _parseCreativeIdFromLogsCommand;
			private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;

			public Job(EventDispatcher eventDispatcher,
				ParseLogsCommand parseLogsCommand,
				ParseCreativeIdFromLogsCommand parseCreativeIdFromLogsCommand,
				LogsStore logsStore,
				OmniRecordManager omniRecordManager,
				IntervalRulesStore intervalRulesStore,
				CreativeFragmentSettings creativeFragmentSettings,
				ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand,
				Logger logger)
			{
				_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
				_parseCreativeIdFromLogsCommand = parseCreativeIdFromLogsCommand;
				_logger = logger;
				_creativeFragmentSettings = creativeFragmentSettings;
				_intervalRulesStore = intervalRulesStore;
				_omniRecordManager = omniRecordManager;
				_logsStore = logsStore;
				_parseLogsCommand = parseLogsCommand;
				_eventDispatcher = eventDispatcher;
			}

			public void Execute(IJobExecutionContext context)
			{
				var mailLogEntries = _logsStore.GetUnprocessedLogs();

				_logger.Info("Found {0} postfix log entries to analyze", mailLogEntries.Count);

				_parseLogsCommand.Logs = mailLogEntries;
				var parsedLogs = _parseLogsCommand.Execute();

				_parseCreativeIdFromLogsCommand.Logs = mailLogEntries;
				var mailIdCreativeIdMaps = _parseCreativeIdFromLogsCommand.Execute();

				parsedLogs = parsedLogs.Join(mailIdCreativeIdMaps, x => x.MessageId, y => y.MessageId, (x, y) =>
					{
						x.CreaiveId = y.CreativeId;
						return x;
					}).ToList();

				parsedLogs = parsedLogs
					.OrderBy(x => x.Time)
					.ToList();

				if (!parsedLogs.Any())
					return;

				var parsedLogsDomainGroups = CalculateDomainGroupFor(parsedLogs);

				var mailSent = ParseToSpecificMailEvent(parsedLogs, MailEventType.Sent, ToMailSent, parsedLogsDomainGroups);
				var mailBounced = ParseToSpecificMailEvent(parsedLogs, MailEventType.Bounced, ToMailBounced, parsedLogsDomainGroups)
					.Concat(ParseToSpecificMailEvent(parsedLogs, MailEventType.Deferred, ToMailBounced, parsedLogsDomainGroups))
					.ToList();

				_logger.Info("postfix log contained: send: {0},bounced: {1}, deferred: {2}", mailSent.Count, mailBounced.Count);

				_omniRecordManager.BatchInsert(mailSent);
				_omniRecordManager.BatchInsert(mailBounced);

				_logger.Info("entries were saved to database");

				_logsStore.MarkProcessedFrom(LastProcessedMailingEvent(parsedLogs));

				DispatchEvent<AggregatedMailBounced, MailBounced>(mailBounced);
				DispatchEvent<AggregatedMailSent, MailSent>(mailSent);
			}

			private static DateTime LastProcessedMailingEvent(IEnumerable<MailEvent> parsedLogs)
			{
				return parsedLogs.Last().Time;
			}

			private IDictionary<string, string> CalculateDomainGroupFor(IEnumerable<MailEvent> mailEvents)
			{
				var conditions = _intervalRulesStore
					.GetAll()
					.SelectMany(intervalRule => intervalRule.Conditons.Select(x => new { Condition = x, intervalRule.Group }))
					.ToList();

				return mailEvents
					.Distinct(new LambdaComparer<MailEvent>((x, y) => x.Recipient == y.Recipient))
					.Select(x => new { Group = conditions.Where(m => x.Recipient.Contains(m.Condition)).Select(m => m.Group).FirstOrDefault(), x.Recipient })
					.ToDictionary(x => x.Recipient, x => x.Group);
			}

			private List<TEventData> ParseToSpecificMailEvent<TEventData>(IEnumerable<MailEvent> parsedLogs, MailEventType mailEventType, Func<MailEvent, TEventData> convertFunction, IEnumerable<KeyValuePair<string, string>> parsedLogsDomainGroups) where TEventData : IHasDomainGroup, IHasRecipient
			{
				return parsedLogs
					.Where(x => x.Type == mailEventType)
					.Select(convertFunction)
					.Join(parsedLogsDomainGroups, x => x.Recipient, y => y.Key, SetDomainGroup)
					.ToList();
			}

			private TEventData SetDomainGroup<TEventData>(TEventData x, KeyValuePair<string, string> y) where TEventData : IHasDomainGroup, IHasRecipient
			{
				x.DomainGroup = y.Value ?? _creativeFragmentSettings.DefaultGroup;
				return x;
			}

			private MailBounced ToMailBounced(MailEvent x)
			{
				return new MailBounced
						   {
							   Recipient = x.Recipient,
							   Time = x.Time,
							   Message = x.RelayMessage,
							   CreativeId = x.CreaiveId,
							   Domain = x.Recipient.GetDomain(),
							   Classification = Classify(x)
						   };
			}

			private MailClassfication Classify(MailEvent x)
			{
				_classifyNonDeliveredMailCommand.Message = x.RelayMessage;
				var mailClassfication = _classifyNonDeliveredMailCommand.Execute();
				return mailClassfication;
			}

			private static MailSent ToMailSent(MailEvent x)
			{
				return new MailSent
						   {
							   Recipient = x.Recipient,
							   Time = x.Time,
							   CreativeId = x.CreaiveId,
							   Domain = x.Recipient.GetDomain()
						   };
			}

			private void DispatchEvent<TEvent, TEventData>(IList<TEventData> mailEvents) where TEvent : AggregatedMailEvents<TEventData>, new()
			{
				if (!mailEvents.Any())
					return;

				var mailEvent = new TEvent
									{
										MailEvents = mailEvents
									};

				_eventDispatcher.ExecuteAll(mailEvent);
			}
		}
	}
}