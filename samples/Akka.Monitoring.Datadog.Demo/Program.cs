using System;
using System.Threading;
using Akka.Actor;
using StatsdClient;

namespace Akka.Monitoring.Datadog.Demo
{
    class HelloActor : ReceiveActor
    {
        protected override void PreStart()
        {
            Context.IncrementActorCreated();
            base.PreStart();
        }

        protected override void PostStop()
        {
            Context.IncrementActorStopped();
            base.PostStop();
        }

        public HelloActor() =>
            Receive<string>(message =>
            {
                Context.IncrementMessagesReceived();

                Console.WriteLine("Received: {0}", message);

                if (message == "Goodbye")
                {
                    Context.Self.Tell(PoisonPill.Instance);
                    Program.ManualResetEvent.Set(); //allow the program to exit
                }
                else
                {
                    Sender.Tell("Hello!");
                }
            });
    }

    class GoodbyeActor : ReceiveActor
    {
        protected override void PreStart()
        {
            Context.IncrementActorCreated();
            base.PreStart();
        }

        protected override void PostStop()
        {
            Context.IncrementActorStopped();
            base.PostStop();
        }

        public GoodbyeActor() =>
            ReceiveAny(message =>
            {
                Context.IncrementMessagesReceived();

                switch (message)
                {
                    case string stringMessage:
                        Console.WriteLine($"Received: {stringMessage}");

                        Sender.Tell("Goodbye");
                        Context.Self.Tell(PoisonPill.Instance);
                        break;

                    case IActorRef target:
                        target.Tell("Starting");
                        break;
                }
            });
    }

    class Program
    {
        public static AutoResetEvent ManualResetEvent = new AutoResetEvent(false);

        private static ActorSystem _system;

        static void Main(string[] args)
        {
            _system = ActorSystem.Create("akka-datadog-demo");

            var statsdConfig = new StatsdConfig
            {
                StatsdServerName = "127.0.0.1"
            };
            var registeredMonitor = ActorMonitoringExtension.RegisterMonitor(_system, new ActorDatadogMonitor(statsdConfig));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Starting up actor system...");

            var goodbye = _system.ActorOf<GoodbyeActor>();
            var hello = _system.ActorOf<HelloActor>();

            Console.WriteLine(registeredMonitor
                ? "Successfully registered StatsD monitor"
                : "Failed to register StatsD monitor");

            Console.WriteLine("Incrementing debug log once every 10 ms for 2 seconds...");

            for (var i = 0; i < 20; i++)
            {
                ActorMonitoringExtension.Monitors(_system).IncrementDebugsLogged();
                Console.WriteLine("Logging debug...");
                Thread.Sleep(10);
            }

            Console.WriteLine("Starting a conversation between actors");
            goodbye.Tell(hello);

            while (ManualResetEvent.WaitOne())
            {
                Console.WriteLine("Shutting down...");

                _system.Terminate();

                Console.WriteLine("Shutdown complete");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();

                return;
            }
        }
    }
}
