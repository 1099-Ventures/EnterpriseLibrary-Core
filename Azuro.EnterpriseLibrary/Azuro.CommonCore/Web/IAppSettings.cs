namespace Azuro.Common.Web
{
	public interface IAppSettings
	{
		public string ApiBaseUrl { get; set; }
		public string FunctionsSecurityKey { get; set; }
		public bool HttpDoNotThrowOnError { get; set; }
	}
}
