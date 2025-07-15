using Azuro.Common.Extensions;
using Azuro.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azuro.Common.Functions
{
	public abstract class AFunction<TFunction>
	{
		private readonly IServiceProvider _serviceProvider;
		protected readonly ILogger<TFunction> _logger;

		protected IServiceProvider ServiceProvider => _serviceProvider;
		protected JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true, };
		protected JsonDocumentOptions JsonDocumentOptions { get; set; } = new JsonDocumentOptions { };

		/// <summary>
		/// Constructor for abstract base class
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="serviceProvider">A reference to the ServiceProvider. Neede for the validation implementation.</param>
		protected AFunction(ILogger<TFunction> logger = null, IServiceProvider serviceProvider = null)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="req"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		protected async Task<T> GetPostBodyAsync<T>(HttpRequest req)
		{
			//	TODO: Investigate Code Contracts - https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/code-contracts
			//		See the Example of CodeContract Attribute class
			//Contract.Requires<ArgumentNullException>(req != null, "HttpRequest parameter cannot be NULL.");
			//Contract.Requires<ArgumentNullException>(log != null, "ILogger parameter cannot be NULL.");
			LogTrace($"Entering {nameof(GetPostBodyAsync)} {typeof(T)}");
			return await JsonSerializer.DeserializeAsync<T>(req.Body, JsonSerializerOptions);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		protected Tuple<bool, List<ValidationResult>> ValidateObject(object value, IDictionary<object, object> items = null)
		{
			if (ServiceProvider == null)
				throw new InvalidOperationException("Service Provider cannot be null when using ValidateObject. Set it on the constructor to the base.");
			if (value == null)
				throw new ArgumentNullException(nameof(value), "Value to be validated cannot be null.");
			var validationResults = new List<ValidationResult>();
			return Tuple.Create(Validator.TryValidateObject(value, new ValidationContext(value, ServiceProvider, items), validationResults), validationResults);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		protected void LogInfo(string message, params object[] args)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogInformation(message, args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ex"></param>
		protected void LogException(Exception ex)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogException(ex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		protected void LogError(string message, params object[] args)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogError(message, args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		protected void LogWarn(string message, params object[] args)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogWarning(message, args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		protected void LogCritical(string message, params object[] args)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogCritical(message, args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		protected void LogDebug(string message, params object[] args)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogDebug(message, args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		protected void LogTrace(string message, params object[] args)
		{
			if (_logger != null)    //	Logger could be null, so ignore logging if it is
				_logger.LogTrace(message, args);
		}

		protected string GetAppSetting(string name)
		{
			//	TODO: Is it always environment vars?
			return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
		}

		protected T GetAppSetting<T>(string name) where T : struct
		{
			//	TODO: Build converter logic (or reference old Util class
			var setting = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

			if (typeof(T) == typeof(int))
			{
				if (int.TryParse(setting, out int result))
					return (T)(object)result;
			}
			else
				return (T)Utility.ChangeType(setting, typeof(T));

			throw new ArgumentException($"Setting [{name}] with value [{setting}] cannot be converted to [{typeof(T)}]");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="req"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetQueryParm(HttpRequest req, string key)
		{
			return req.Query.ContainsKey(key) ? (string)req.Query[key] : null;
		}

		/// <summary>
		/// Logs the Exception and returns a BadRequestObjectResult with the Exception Message.
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		protected ObjectResult PackageErrorResult(Exception ex)
		{
			LogException(ex);
			return new BadRequestObjectResult(ex.Message);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		protected async Task<ClaimsPrincipal> GetPrincipalAsync(HttpRequest req)
		{
			return req.Headers.ContainsKey(Http.Constants.ClaimsPrincipalHeader)
				? (await DeserializeHeader<JsonClaimsPrincipal>(req, Http.Constants.ClaimsPrincipalHeader)).ToClaimsPrincipal()
				: null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="req"></param>
		/// <param name="header"></param>
		/// <returns></returns>
		protected async Task<T> DeserializeHeader<T>(HttpRequest req, string header)
		{
			return await JsonSerializer.DeserializeAsync<T>(new MemoryStream(await Task.FromResult(Encoding.UTF8.GetBytes(req.Headers[header].ToString()))));
		}
	}
}