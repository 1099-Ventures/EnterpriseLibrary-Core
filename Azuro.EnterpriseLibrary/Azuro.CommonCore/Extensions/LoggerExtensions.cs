using Microsoft.Extensions.Logging;
using System;

namespace Azuro.Common.Extensions
{
	public static class LoggerExtensions
	{
		public static void LogException(this ILogger logger, Exception ex)
		{
			logger.LogException($"Exception from [{(ex.Source ?? ex.TargetSite?.Name)}]", ex);
		}

		public static void LogException(this ILogger logger, string message, Exception ex)
		{
			logger.LogError($"{message}:{Environment.NewLine}{Utility.UnpackException(ex)}");
		}
	}
}
