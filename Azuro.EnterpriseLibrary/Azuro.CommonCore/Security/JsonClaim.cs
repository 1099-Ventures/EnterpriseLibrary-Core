using System.Text.Json.Serialization;

namespace Azuro.Common.Security
{
	/// <summary>
	/// Use shortened property names to save some space in the serialized object.
	/// </summary>
	public class JsonClaim
	{
		[JsonPropertyName("t")]
		public string Type { get; set; }
		[JsonPropertyName("v")]
		public string Value { get; set; }
		[JsonPropertyName("vt")]
		public string ValueType { get; set; }
		[JsonPropertyName("i")]
		public string Issuer { get; set; }
		[JsonPropertyName("oi")]
		public string OriginalIssuer { get; set; }
	}
}
