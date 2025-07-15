using Azuro.Common.AzureStorage;

namespace Azuro.EnterpriseLibrary.ConsoleTester
{
	public class BlobStorageOptions : IBlobStorageOptions
	{
		public bool UseDevelopment { get; set; }
		public string Connection { get; set; }
		public string ContainerName { get; set; }
		public string StaticHostUrl { get; set; }
		public bool AzureCredentials { get; set; }
		public bool IsPublic { get; set; }
	}
}
