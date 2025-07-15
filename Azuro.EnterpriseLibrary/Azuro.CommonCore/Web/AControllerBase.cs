using Azuro.Common.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Azuro.Common.Web
{
	public abstract class AControllerBase<TController, TAppSettings>
		: ControllerBase
		where TController : class
		where TAppSettings : class, IAppSettings
	{
		protected readonly ILogger<TController> _logger;
		protected readonly IConfiguration _config;
		protected readonly TAppSettings _appSettings;
		protected readonly HttpInjectableHelper _httpHelper;
		protected string ApiBaseUrl => _appSettings.ApiBaseUrl ?? _config[nameof(ApiBaseUrl)];
		protected string FunctionsSecurityKey => _appSettings?.FunctionsSecurityKey ?? _config[nameof(FunctionsSecurityKey)];
		protected Uri ApiBaseUri => new(ApiBaseUrl);

		public AControllerBase(ILogger<TController> logger
			, IConfiguration configuration
			, IOptions<TAppSettings> appSettings
			, HttpInjectableHelper http)
		{
			_logger = logger;
			_config = configuration;
			_appSettings = appSettings?.Value;
			_httpHelper = http;

			//	Inverted to avoid changing default behaviour
			_httpHelper.ThrowOnError = !_appSettings.HttpDoNotThrowOnError;
		}

		protected string CreateFunctionUrl(string path, object[] routeParms = null, Tuple<string, string>[] queryParms = null)
		{
			var builder = new UriBuilder(new Uri(ApiBaseUri, path));

			if (routeParms != null)
			{
				foreach (var rp in routeParms)
					builder.Path += $"/{rp}";
			}

			if (!string.IsNullOrWhiteSpace(FunctionsSecurityKey))
			{
				//	Changed it to make use of Headers instead of Query Parameters
				_httpHelper.AddFunctionsSecurityKey(FunctionsSecurityKey);
			}

			_httpHelper.AppendClaimsPrincipal(User);

			CreateQueryString(builder, queryParms);

			_logger.LogDebug($"Functions Url: {builder}");

			return builder.ToString();
		}

		protected void CreateQueryString(UriBuilder builder, Tuple<string, string>[] queryParms = null)
		{
			if (queryParms != null)
			{
				foreach (var q in queryParms)
				{
					builder.Query += $"{(builder.Query.Length > 0 ? "&" : string.Empty)}{q.Item1}={System.Net.WebUtility.UrlEncode(q.Item2)}";
				}
			}
		}

		protected IActionResult UnpackException(Exception ex)
		{
			var message = $"Exception in {GetType().Name}: {Utility.UnpackException(ex)}";
			_logger.LogError(message);
			return BadRequest(message);
		}

		protected bool IsHttpSuccess(System.Net.HttpStatusCode? status) => status != null && (int)status >= 200 && (int)status < 300;

		protected IActionResult HandleApiResult<TResult>(Tuple<HttpResult, TResult> result, string message = "There was an error processing the request.") where TResult : class
		{
			//	'result' should never be null in the call to Ok, however SC identifies a false positive for a possible null value.
			return IsHttpSuccess(result?.Item1?.StatusCode)
					? Ok(result?.Item2)
					: BadRequest($"{message}: {result?.Item1?.Error}");
		}
	}
}
