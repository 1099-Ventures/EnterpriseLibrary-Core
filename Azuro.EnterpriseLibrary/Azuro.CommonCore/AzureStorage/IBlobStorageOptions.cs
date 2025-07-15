namespace Azuro.Common.AzureStorage
{
	public interface IBlobStorageOptions
	{
		bool UseDevelopment { get; set; }
		string Connection { get; set; }
		string ContainerName { get; set; }
		string StaticHostUrl { get; set; }
		bool AzureCredentials { get; set; }
		bool IsPublic { get; set; }
	}
}
