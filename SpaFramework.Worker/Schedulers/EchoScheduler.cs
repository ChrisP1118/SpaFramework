using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.Models.Service.WorkItems.Echo;
using SpaFramework.App.Services.WorkItems;
using NodaTime;
using System.Threading;
using System.Threading.Tasks;

namespace SpaFramework.Worker.Schedulers
{
    public class EchoSheduler
    {
        private readonly IConfiguration _configuration;
        private readonly IClock _clock;
        private readonly IWorkItemService<EchoWorkItem> _echoWorkItemService;

        public EchoSheduler(IConfiguration configuration, IClock clock, IWorkItemService<EchoWorkItem> echoWorkItemService)
        {
            _configuration = configuration;
            _clock = clock;
            _echoWorkItemService = echoWorkItemService;
        }

        /// <summary>
        /// Runs every minute
        /// </summary>
        /// <param name="timerInfo"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Trigger([TimerTrigger("0 */1 * * * *", RunOnStartup = false, UseMonitor = true)] TimerInfo timerInfo, ILogger logger, CancellationToken cancellationToken)
        {
            Instant now = _clock.GetCurrentInstant();

            await _echoWorkItemService.EnqueueWorkItem(new EchoWorkItem()
            {
                Message = $"The time is {now.ToString("HH:mm:ss", null)}"
            });
        }
    }
}
