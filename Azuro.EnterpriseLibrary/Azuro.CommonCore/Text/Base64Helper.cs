using System;
using System.Text;
using System.Threading.Tasks;

namespace Azuro.Common.Text
{
	public static class Base64StringExtensions
	{
		public static string EnsureBase64(this string source)
		{
			StringBuilder contentBuilder = new();
			contentBuilder.Append(source);
			//base64 should be multiple of 4.if not, its not a valid base64 string.
			//This just adds 'spacers' at the end if needed.
			while (source.Length % 4 != 0)
			{
				contentBuilder.Append('=');
			}

			return contentBuilder.ToString();
		}

		public static async Task<string> ToBase64Async(this byte[] bytes)
		{
			return await Task.FromResult(Convert.ToBase64String(bytes).EnsureBase64());
		}
	}
}
