using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azuro.Common.Http
{
	public class HttpResult
	{
		public HttpStatusCode StatusCode { get; set; }
		public string ReasonPhrase { get; set; }
		public string Error { get; set; }

		public async static Task<HttpResult> Create(HttpResponseMessage response)
		{
			return new HttpResult
			{
				StatusCode = response.StatusCode,
				ReasonPhrase = response.ReasonPhrase,
				Error = !response.IsSuccessStatusCode ? await response.Content?.ReadAsStringAsync() ?? "Unknown Error" : null,
			};
		}
	}
}
