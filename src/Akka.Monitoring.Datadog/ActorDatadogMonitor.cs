using System;
using System.Collections.Concurrent;
using Akka.Monitoring.Impl;
using StatsdClient;

namespace Akka.Monitoring.Datadog
{
    public class ActorDatadogMonitor : AbstractActorMonitoringClient
    {
        private static readonly Guid MonitorName = new Guid("674378D3-E06B-4B02-8AE5-7C3B45DDA59E");
		private const string AkkaMetricPrefix = "akka.";

		private readonly ConcurrentDictionary<string, string> _metricNames = new ConcurrentDictionary<string, string>();
		private readonly ConcurrentDictionary<string, string[]> _tags = new ConcurrentDictionary<string, string[]>();
        private readonly DogStatsdService _dogStatsdService = new DogStatsdService();

        public ActorDatadogMonitor(StatsdConfig config) => 
            _dogStatsdService.Configure(config);

        public override int MonitoringClientId => MonitorName.GetHashCode();

        public override void UpdateCounter(string metricName, int delta, double sampleRate) =>
            _dogStatsdService.Increment(GetDatadogMetricName(metricName), delta, sampleRate, GetDatadogTags(metricName));

        public override void UpdateGauge(string metricName, int value, double sampleRate) =>
            _dogStatsdService.Gauge(GetDatadogMetricName(metricName), value, sampleRate, GetDatadogTags(metricName));

        public override void UpdateTiming(string metricName, long time, double sampleRate) =>
            _dogStatsdService.Timer(GetDatadogMetricName(metricName), time, sampleRate, GetDatadogTags(metricName));

		internal string GetDatadogMetricName(string akkaMetricName) =>
            _metricNames.GetOrAdd(akkaMetricName ?? "", _ =>
            {
                if (string.IsNullOrWhiteSpace(akkaMetricName))
                    return "";

				if (akkaMetricName.StartsWith(AkkaMetricPrefix, StringComparison.InvariantCulture))
                    return $"{AkkaMetricPrefix}global.{akkaMetricName.Substring(AkkaMetricPrefix.Length)}";

                var akkaMetricIndex = akkaMetricName.LastIndexOf(AkkaMetricPrefix, StringComparison.InvariantCulture);

                return akkaMetricName.Substring(akkaMetricIndex);
            });

        internal string[] GetDatadogTags(string akkaMetricName) =>
            _tags.GetOrAdd(akkaMetricName ?? "", _ =>
            {
                if (string.IsNullOrWhiteSpace(akkaMetricName) || akkaMetricName.StartsWith(AkkaMetricPrefix, StringComparison.InvariantCulture))
                    return new string[] { };

                var akkaMetricIndex = akkaMetricName.LastIndexOf(AkkaMetricPrefix, StringComparison.InvariantCulture);
                var metricContext = akkaMetricName.Substring(0, akkaMetricIndex - 1);
                var typeIndex = metricContext.LastIndexOf(".", StringComparison.InvariantCulture) + 1;

                var actorSystem = metricContext.Substring(0, typeIndex - 1);
                var actorType = metricContext.Substring(typeIndex);

                return new[] 
                {
                    $"actorsystem:{actorSystem}",
                    $"actortype:{actorType}"
                };
            });

		public override void DisposeInternal() { }
    }
}
