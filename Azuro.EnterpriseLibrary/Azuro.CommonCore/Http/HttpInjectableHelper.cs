using Azuro.Common.Extensions;
using Azuro.Common.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azuro.Common.Http
{
	public class HttpInjectableHelper
	{
		private readonly IHttpClientFactory _factory;
		private readonly ILogger _logger;

		public JsonSerializerOptions JsonSerializerOptions { get; private set; } = new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault, PropertyNameCaseInsensitive = true, };
		public JsonDocumentOptions JsonDocumentOptions { get; private set; } = new() { };
		public bool ThrowOnError { get; set; } = true;
		private readonly Dictionary<string, string> _headers = new();

		public HttpInjectableHelper(IHttpClientFactory httpClientFactory, ILogger<HttpInjectableHelper> logger)
		{
			_factory = httpClientFactory;
			_logger = logger;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public async Task<JsonDocument> GetJsonAsync(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader, true);

			return EnsureSuccessStatusCode(responseMsg)
				? await JsonDocument.ParseAsync(await responseMsg.Content.ReadAsStreamAsync(), JsonDocumentOptions)
				: null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public async Task<T> GetJsonAsync<T>(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader, true);

			return EnsureSuccessStatusCode(responseMsg)
				? await JsonSerializer.DeserializeAsync<T>(await responseMsg.Content.ReadAsStreamAsync(), JsonSerializerOptions)
				: default;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public async Task<string> GetStringAsync(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader);

			return EnsureSuccessStatusCode(responseMsg) ? await responseMsg.Content.ReadAsStringAsync() : null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public async Task<byte[]> GetByteArrayAsync(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader);
			return EnsureSuccessStatusCode(responseMsg) ? await responseMsg.Content.ReadAsByteArrayAsync() : null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="TInput"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <returns>A Tuple of HttpResult and T2</returns>
		public async Task<Tuple<HttpResult, TResult>> PostJsonAsync<TInput, TResult>(string url, string authHeader, TInput input) where TInput : class where TResult : class
		{
			var responseMsg = await VerbAsync(HttpMethod.Post, url, authHeader, SerializeJson(input), true);

			return EnsureSuccessStatusCode(responseMsg)
				? new Tuple<HttpResult, TResult>(await HttpResult.Create(responseMsg), await DeserializeAsync<TResult>(responseMsg))
				: new Tuple<HttpResult, TResult>(await HttpResult.Create(responseMsg), default);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="TInput"></typeparam>
		/// <returns>A Tuple of HttpResult and string</returns>
		public async Task<Tuple<HttpResult, string>> PostJsonAsync<TInput>(string url, string authHeader, TInput input)
		{
			var responseMsg = await VerbAsync(HttpMethod.Post, url, authHeader, SerializeJson(input), true);

			return EnsureSuccessStatusCode(responseMsg)
				? new Tuple<HttpResult, string>(await HttpResult.Create(responseMsg), await responseMsg.Content.ReadAsStringAsync())
				: new Tuple<HttpResult, string>(await HttpResult.Create(responseMsg), default);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="TInput"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <returns>A Tuple of HttpResult and T2</returns>
		public async Task<Tuple<HttpResult, TResult>> PatchJsonAsync<TInput, TResult>(string url, string authHeader, TInput input)
		{
			var responseMsg = await VerbAsync(HttpMethod.Patch, url, authHeader, SerializeJson(input), true);

			return EnsureSuccessStatusCode(responseMsg)
				? new Tuple<HttpResult, TResult>(await HttpResult.Create(responseMsg), await DeserializeAsync<TResult>(responseMsg))
				: new Tuple<HttpResult, TResult>(await HttpResult.Create(responseMsg), default);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="TInput"></typeparam>
		/// <returns></returns>
		public async Task<Tuple<HttpResult, string>> PatchJsonAsync<TInput>(string url, string authHeader, TInput input, Dictionary<string, string> headers = null)
		{
			var responseMsg = await VerbAsync(HttpMethod.Patch, url, authHeader, SerializeJson(input), true, headers);

			return EnsureSuccessStatusCode(responseMsg)
				? new Tuple<HttpResult, string>(await HttpResult.Create(responseMsg), await responseMsg.Content.ReadAsStringAsync())
				: new Tuple<HttpResult, string>(await HttpResult.Create(responseMsg), await responseMsg.Content.ReadAsStringAsync());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="principal"></param>
		public void AppendClaimsPrincipal(ClaimsPrincipal principal)
		{
			if (principal != null)
			{
				var jsonPrincipal = new JsonClaimsPrincipal(principal);
				_headers.Add(Constants.ClaimsPrincipalHeader, SerializeJson(jsonPrincipal));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="functionsKey"></param>
		public void AddFunctionsSecurityKey(string functionsKey)
		{
			_headers.Add(Constants.FunctionsKeyHeader, functionsKey);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="appJson"></param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> GetResponseAsync(string url, string authHeader, bool appJson = false)
		{
			return await VerbAsync(HttpMethod.Get, url, authHeader, null, appJson);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="body"></param>
		/// <param name="appJson"></param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> VerbAsync(HttpMethod method, string url, string authHeader, string body, bool appJson = false, Dictionary<string, string> headers = null)
		{
			try
			{
				using var client = _factory.CreateClient();
				using HttpRequestMessage request = new(method, url);
				if (authHeader != null)
					request.Headers.TryAddWithoutValidation("Authorization", authHeader);

				if (appJson)
				{
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					request.Content = !string.IsNullOrWhiteSpace(body) ? new StringContent(body, Encoding.UTF8, "application/json") : null;
				}
				else
				{
					request.Content = !string.IsNullOrWhiteSpace(body) ? new StringContent(body) : null;
				}

				if (headers?.Count > 0)
				{
					foreach (var h in headers)
						request.Headers.TryAddWithoutValidation(h.Key, h.Value);
				}

				if (_headers.Count > 0)
				{
					foreach (var h in _headers)
						request.Headers.TryAddWithoutValidation(h.Key, h.Value);
				}

				return await client.SendAsync(request);
			}
			catch (Exception ex)
			{
				_logger.LogException(nameof(VerbAsync), ex);
				//	rethrow
				throw;
			}
			finally
			{
				//	Clear out the Headers
				_headers.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		private bool EnsureSuccessStatusCode(HttpResponseMessage response)
		{
			if (ThrowOnError)
				response.EnsureSuccessStatusCode();

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		private async Task<TResult> DeserializeAsync<TResult>(HttpResponseMessage response)
		{
			return await JsonSerializer.DeserializeAsync<TResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TInput"></typeparam>
		/// <param name="input"></param>
		/// <returns>A serialized JSON string.</returns>
		private string SerializeJson<TInput>(TInput input)
		{
			return JsonSerializer.Serialize(input, JsonSerializerOptions);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="responseMsg"></param>
		/// <returns></returns>
		private async Task<Tuple<HttpResult, TResult>> PostResponse<TResult>(HttpResponseMessage responseMsg) where TResult : class
		{
			return EnsureSuccessStatusCode(responseMsg)
				? new Tuple<HttpResult, TResult>(await HttpResult.Create(responseMsg), await ReadResponse<TResult>(responseMsg))
				: new Tuple<HttpResult, TResult>(await HttpResult.Create(responseMsg), default);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="responseMsg"></param>
		/// <returns></returns>
		private async Task<TResult> ReadResponse<TResult>(HttpResponseMessage responseMsg) where TResult : class
		{
			return typeof(TResult) == typeof(string)
				? await responseMsg.Content.ReadAsStringAsync() as TResult
				: await DeserializeAsync<TResult>(responseMsg);
		}

	}
}