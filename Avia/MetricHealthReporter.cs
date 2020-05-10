using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Health;

namespace Avia
{
    public class MetricHealthReporter : IReportHealthStatus
    {
        private IMetricsRoot _metrics;

        public MetricHealthReporter(IMetricsRoot metrics)
        {
            _metrics = metrics;
        }

        public TimeSpan ReportInterval { get; set; }

        public Task ReportAsync(HealthOptions options, HealthStatus status, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var item in status.Results)
            {
                _metrics.Measure.Counter.Increment(new CounterOptions
                {
                    Name = "health",
                    MeasurementUnit = Unit.Calls,
                    Tags = new MetricTags(
                        new[] { "name", "status" },
                        new[] { item.Name, item.Check.Status.ToString() })
                });
            }

            return Task.CompletedTask;
        }
    }
}