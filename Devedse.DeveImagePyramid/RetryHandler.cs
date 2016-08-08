using Devedse.DeveImagePyramid.Logging;
using System;

namespace Devedse.DeveImagePyramid
{
    public class RetryHandler
    {
        private readonly ILogger _logger;

        public RetryHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Retry(Action actionToExecute, int times)
        {
            while (times > 0)
            {
                times--;
                try
                {
                    actionToExecute();
                    return;
                }
                catch (Exception ex)
                {
                    if (times == 0)
                    {
                        _logger.WriteError($"The exception that occured has been retryed but kept on failing. Throwing again... Exception: {ex}", LogLevel.Exception);
                        throw;
                    }
                    else
                    {
                        _logger.WriteError($"An exception occured, retrying {times} more times. Exception: {ex}", LogLevel.Error);
                    }
                }
            }
        }
    }
}
