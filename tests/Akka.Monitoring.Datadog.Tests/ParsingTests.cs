using StatsdClient;
using Xunit;

namespace Akka.Monitoring.Datadog.Tests
{
    public class ParsingTests
    {
        private readonly ActorDatadogMonitor _monitor = new ActorDatadogMonitor(new StatsdConfig
        {
            StatsdServerName = "127.0.0.1"
        });

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("akka.actor.restarts", "akka.global.actor.restarts")]
        [InlineData("system.actor.akka.actor.restarts", "akka.actor.restarts")]
        [InlineData("dotted.system.actor.akka.actor.restarts", "akka.actor.restarts")]
        public void GetDatadogMetricName(string akkaMetricName, string expectedDatadogMetricName)
        {
            var firstParse = _monitor.GetDatadogMetricName(akkaMetricName);
            Assert.Equal(expectedDatadogMetricName, firstParse);

            var secondParse = _monitor.GetDatadogMetricName(akkaMetricName);
            Assert.Equal(expectedDatadogMetricName, secondParse);
        }

        [Theory]
        [InlineData("", new string[] { })]
        [InlineData(null, new string[] { })]
        [InlineData("akka.actor.restarts", new string[] { })]
        [InlineData("system.actor.akka.actor.restarts", new[] { "actorsystem:system", "actortype:actor" })]
        [InlineData("dotted.system.actor.akka.actor.restarts", new[] { "actorsystem:dotted.system", "actortype:actor" })]
        public void GetDatadogTags(string akkaMetricName, string[] expectedDatadogMetricName)
        {
            var firstParse = _monitor.GetDatadogTags(akkaMetricName);
            Assert.Equal(expectedDatadogMetricName, firstParse);

            var secondParse = _monitor.GetDatadogTags(akkaMetricName);
            Assert.Equal(expectedDatadogMetricName, secondParse);
        }
    }
}
