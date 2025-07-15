using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azuro.Common.Http
{
	[Obsolete("Use HttpInjectableHelper instead")]
	public static class HttpRequestHelper
	{
		public static JsonSerializerOptions JsonSerializerOptions => new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault, PropertyNameCaseInsensitive = true, };
		public static JsonDocumentOptions JsonDocumentOptions => new() { };

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public static async Task<JsonDocument> GetJsonAsync(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader, true);

			responseMsg.EnsureSuccessStatusCode();

			return await JsonDocument.ParseAsync(await responseMsg.Content.ReadAsStreamAsync(), JsonDocumentOptions);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public static async Task<T> GetJsonAsync<T>(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader, true);

			responseMsg.EnsureSuccessStatusCode();

			return await JsonSerializer.DeserializeAsync<T>(await responseMsg.Content.ReadAsStreamAsync(), JsonSerializerOptions);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public static async Task<string> GetStringAsync(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader);

			responseMsg.EnsureSuccessStatusCode();

			return await responseMsg.Content.ReadAsStringAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public static async Task<byte[]> GetByteArrayAsync(string url, string authHeader)
		{
			var responseMsg = await GetResponseAsync(url, authHeader);

			responseMsg.EnsureSuccessStatusCode();

			return await responseMsg.Content.ReadAsByteArrayAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <returns>A Tuple of HttpStatusCode and T2</returns>
		public static async Task<Tuple<HttpStatusCode, T2>> PostJsonAsync<T1, T2>(string url, string authHeader, T1 input)
		{
			var responseMsg = await VerbAsync(HttpMethod.Post, url, authHeader, JsonSerializer.Serialize(input), true);

			if (!responseMsg.IsSuccessStatusCode && responseMsg.Content.Headers.ContentLength == 0)
				return new Tuple<HttpStatusCode, T2>(responseMsg.StatusCode, default);

			return new Tuple<HttpStatusCode, T2>(responseMsg.StatusCode, await JsonSerializer.DeserializeAsync<T2>(await responseMsg.Content.ReadAsStreamAsync(), JsonSerializerOptions));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="T1"></typeparam>
		/// <returns>A Tuple of HttpStatusCode and string</returns>
		public static async Task<Tuple<HttpStatusCode, string>> PostJsonAsync<T1>(string url, string authHeader, T1 input)
		{
			var responseMsg = await VerbAsync(HttpMethod.Post, url, authHeader, JsonSerializer.Serialize(input), true);

			if (!responseMsg.IsSuccessStatusCode && responseMsg.Content.Headers.ContentLength == 0)
				return new Tuple<HttpStatusCode, string>(responseMsg.StatusCode, default);

			return new Tuple<HttpStatusCode, string>(responseMsg.StatusCode, await responseMsg.Content.ReadAsStringAsync());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public static async Task<HttpStatusCode> PostJsonAsync(string url, string authHeader)
		{
			var responseMsg = await VerbAsync(HttpMethod.Post, url, authHeader, null, false);

			return responseMsg.StatusCode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <returns>A Tuple of HttpStatusCode and T2</returns>
		public static async Task<Tuple<HttpStatusCode, T2>> PatchJsonAsync<T1, T2>(string url, string authHeader, T1 input)
		{
			try
			{
				var responseMsg = await VerbAsync(HttpMethod.Patch, url, authHeader, JsonSerializer.Serialize(input), true);

				if (!responseMsg.IsSuccessStatusCode && responseMsg.Content.Headers.ContentLength == 0)
					return new Tuple<HttpStatusCode, T2>(responseMsg.StatusCode, default);

				return new Tuple<HttpStatusCode, T2>(responseMsg.StatusCode, await JsonSerializer.DeserializeAsync<T2>(await responseMsg.Content.ReadAsStreamAsync(), JsonSerializerOptions));
			}
			catch (Exception ex)
			{
				//	TODO: Inject a logger somehow here.
				//	How the hell must I stick the response here?
				throw new Exception($"Exception in {nameof(HttpRequestHelper)}", ex);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="input"></param>
		/// <typeparam name="T1"></typeparam>
		/// <returns></returns>
		public static async Task<Tuple<HttpStatusCode, string>> PatchJsonAsync<T1>(string url, string authHeader, T1 input)
		{
			try
			{
				var responseMsg = await VerbAsync(HttpMethod.Patch, url, authHeader, JsonSerializer.Serialize(input), true);

				if (!responseMsg.IsSuccessStatusCode && responseMsg.Content.Headers.ContentLength == 0)
					return new Tuple<HttpStatusCode, string>(responseMsg.StatusCode, default);

				return new Tuple<HttpStatusCode, string>(responseMsg.StatusCode, await responseMsg.Content.ReadAsStringAsync());
			}
			catch (Exception ex)
			{
				//	TODO: Inject a logger somehow here.
				//	How the hell must I stick the response here?
				throw new Exception($"Exception in {nameof(HttpRequestHelper)}", ex);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public static async Task<HttpStatusCode> DeleteJsonAsync(string url, string authHeader, Tuple<string, string>[] customHeaders = null)
		{
			try
			{
				var responseMsg = await VerbAsync(HttpMethod.Delete, url, authHeader, null, false, customHeaders);

				return responseMsg.StatusCode;
			}
			catch (Exception ex)
			{
				//	TODO: Inject a logger somehow here.
				//	How the hell must I stick the response here?
				throw new Exception($"Exception in {nameof(HttpRequestHelper)}", ex);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="authHeader"></param>
		/// <param name="appJson"></param>
		/// <returns></returns>
		private static async Task<HttpResponseMessage> GetResponseAsync(string url, string authHeader, bool appJson = false)
		{
			var response = await VerbAsync(HttpMethod.Get, url, authHeader, null, appJson);
			response.EnsureSuccessStatusCode();
			return response;
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
		private static async Task<HttpResponseMessage> VerbAsync(HttpMethod method, string url, string authHeader, string body, bool appJson = false, Tuple<string, string>[] customHeaders = null)
		{
			using var client = new HttpClient();
			using HttpRequestMessage request = new HttpRequestMessage(method, url);

			if (authHeader != null)
				request.Headers.TryAddWithoutValidation("Authorization", authHeader);

			if (customHeaders != null)
			{
				foreach (var ch in customHeaders)
					request.Headers.TryAddWithoutValidation(ch.Item1, ch.Item2);
			}

			if (appJson)
			{
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				request.Content = !string.IsNullOrWhiteSpace(body) ? new StringContent(body, Encoding.UTF8, "application/json") : null;
			}
			else
			{
				request.Content = !string.IsNullOrWhiteSpace(body) ? new StringContent(body) : null;
			}

			return await client.SendAsync(request);
		}
	}
}