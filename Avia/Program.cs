using System.Linq;
using App.Metrics;
using App.Metrics.Health;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using App.Metrics.AspNetCore.Health;
using System.Threading;
using App.Metrics.Counter;

namespace Avia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            Metrics = new MetricsBuilder()
                //.Report.ToConsole()
                .OutputMetrics.AsPrometheusPlainText()
                .Configuration.Configure(p =>
                {
                    p.DefaultContextLabel = "Application";
                    p.GlobalTags.Add("app", "avia");
                    p.Enabled = true;
                    p.ReportingEnabled = true;
                })
            .Build();

            return
               WebHost.CreateDefaultBuilder(args)
               .ConfigureMetrics(Metrics)
               .ConfigureHealth(
                    builder =>
                    {
                        builder.OutputHealth.AsPlainText();

                        builder.HealthChecks.AddCheck("Database connect", () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy("Database Connection OK")));
                        //builder.HealthChecks.AddHttpGetCheck("github", new Uri("https://github.com/"), TimeSpan.FromSeconds(1));

                        builder.Report.Using(new MetricHealthReporter(Metrics) { ReportInterval = TimeSpan.FromSeconds(5) });
                    })
                .UseHealth()
                .UseMetrics(
                    options =>
                    {
                        options.EndpointOptions = endpointsOptions =>
                        {
                            endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.First(p => p is MetricsPrometheusTextOutputFormatter);
                        };
                    })
                .UseStartup<Startup>();
        }

        internal static IMetricsRoot Metrics { get; set; }
    }
}
