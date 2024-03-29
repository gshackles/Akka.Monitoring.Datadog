# Akka.Monitoring.Datadog

![Build Status](https://github.com/gshackles/Akka.Monitoring.Datadog/workflows/CI/badge.svg)

This is a plugin for [Akka.Monitoring](https://github.com/petabridge/akka-monitoring) that reports metrics into [Datadog](https://datadoghq.com).

## Usage

You can check out the [sample project](https://github.com/gshackles/Akka.Monitoring.Datadog/tree/main/samples/Akka.Monitoring.Datadog.Demo) to see a quick example of the monitor in action.

To register the monitor with your system:

```
var statsdConfig = new StatsdConfig
{
    StatsdServerName = "127.0.0.1"
};
var registeredMonitor = ActorMonitoringExtension.RegisterMonitor(system, new ActorDatadogMonitor(statsdConfig));
```

You can then interact with Akka.Monitoring as you normally would, and metrics will flow through your Datadog agent. The main difference currently between this and the standard StatsD monitor is that this will attempt to parse the incoming metric names reported by Akka.Monitoring and split them up into better names and tags.

For example, if your actor system is named `mysystem` and your actor is named `myactor`, Akka.Monitoring will report a restart metric for it as `mysystem.myactor.akka.actor.restarts`. Akka.Monitoring.Datadog will report the metric name as `akka.actor.restarts`, and include the tags `actorsystem:mysystem` and `actortype:myactor`.

For some more examples on how the parsing is set up, you can refer to the [parsing tests](https://github.com/gshackles/Akka.Monitoring.Datadog/blob/main/tests/Akka.Monitoring.Datadog.Tests/ParsingTests.cs).
