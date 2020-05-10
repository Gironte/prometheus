using System;
using System.Threading;
using System.Threading.Tasks;
using Avia.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avia
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, ITicketsService ticketService)
        {
            _logger = logger;
            _ticketsService = ticketService;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var tickets = await _ticketsService.GetTicketsAsync();

                _logger.LogInformation(tickets, DateTimeOffset.Now);

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private readonly ITicketsService _ticketsService;
    }
}
