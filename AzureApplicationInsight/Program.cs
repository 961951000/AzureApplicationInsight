using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace AzureApplicationInsight
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var telemetryConfiguration = new TelemetryConfiguration("InstrumentationKey");
            telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            var telemetryClient = new TelemetryClient(telemetryConfiguration);

            await RequestTelemetryAsync(telemetryClient, async operation =>
            {
                Console.WriteLine("Hello!");
                await Task.Delay(1000 * 60);
            }, "Queue Request", new Dictionary<string, string>());

            Console.WriteLine("World!");
            Console.ReadLine();
        }

        public static async Task RequestTelemetryAsync(TelemetryClient telemetryClient, Func<OperationContext, Task> action, string operationName, IDictionary<string, string> properties)
        {
            var operationId = Guid.NewGuid().ToString("N");
            // Establish an operation context and associated telemetry item:
            using (var operation = telemetryClient.StartOperation<RequestTelemetry>(operationName, operationId))
            {
                var operationContext = operation.Telemetry.Context.Operation;

                // Set properties of containing telemetry item.
                if (properties != null)
                {
                    foreach (var kv in properties)
                    {
                        operation.Telemetry.Properties.TryAdd(kv.Key, kv.Value);
                    }
                }

                // Telemetry sent in here will use the same operation ID.
                await action?.Invoke(operationContext);

                // Optional: explicitly send telemetry item.
                telemetryClient.StopOperation(operation);

            } // When operation is disposed, telemetry item is sent.
        }
    }
}
