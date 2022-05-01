using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class ServicesVideoWorker : BackgroundService
    {
        private readonly ILogger<ServicesVideoWorker> _logger;

        public ServicesVideoWorker(ILogger<ServicesVideoWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // check the disk space

                // get the list of files 

                // uncompress the file

                // extract the file contents to working directory

                

                // if any files are not in mp4 format, then convert them 


                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }
    }
}