using System;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using AviaTicketsFinder.Models;
using Microsoft.Extensions.Options;

namespace Avia.Services
{
    public class AviaSalesService : ITicketsService
    {
        public AviaSalesService(IHttpClientWrapper httpClientWrapper, IOptions<Settings> settings, IMetricsRoot metricsRoot)
        {
            _httpClient = httpClientWrapper;
            _settings = settings.Value;
            _metrics = metricsRoot;
        }

        public async Task<string> GetTicketsAsync()
        {
            IncrementCustom("test");
            return await _httpClient.Get("https://api-gateway.travelata.ru/statistic/cheapestTours");
        }

        private void IncrementCustom(string str)
        {
            _metrics.Measure.Counter.Increment(new CounterOptions
            {
                Name = "custom",
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags("val", str)
            });
        }

        private readonly IMetricsRoot _metrics;
        private readonly Settings _settings;
        private IHttpClientWrapper _httpClient;
    }
}