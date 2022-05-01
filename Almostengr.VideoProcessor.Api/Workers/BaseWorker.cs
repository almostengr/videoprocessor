using System;
using System.IO;
using Almostengr.VideoProcessor.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public abstract class BaseWorker : BackgroundService
    {
        private readonly ILogger<BaseWorker> _logger;

        protected BaseWorker(ILogger<BaseWorker> logger)
        {
            _logger = logger;
        }

    } // end class
}