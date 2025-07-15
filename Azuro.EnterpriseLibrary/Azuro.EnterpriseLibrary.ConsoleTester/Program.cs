using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azuro.Common.AzureStorage;
using Azuro.Common.Http;
using Azuro.Common.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azuro.EnterpriseLibrary.ConsoleTester
{
	class Program
	{
		private static IConfigurationRoot _config;
		private static ServiceProvider _services;

		[SuppressMessage("Microsoft.Design", "IDE0060")]
		static void Main(string[] args)
		{
			//var secret = GetSecret("devops-azuro-pat");

			//Console.WriteLine(secret != null);

			//PostTest();

			var serviceCollection = new ServiceCollection();
			_services = ConfigureServices(serviceCollection);

			//StringTest();

			//BlobFetchTest();

			//BlobUploadTest();

			//BlobCopyTest();

			GetExpiredSecrets().Wait();
		}

		private static void BlobUploadTest()
		{
			var blobHelper = _services.GetService<BlobStorageHelper<BlobStorageOptions>>();
			var localFilePath = @"C:\Users\johann.ungerer\OneDrive - Azuro Solutions\Pictures\Get together at Mic\IMG-20200703-WA0009.jpg";
			var imagePath = "images/e_sig_banner.jpg";

			var bytes = File.ReadAllBytes(localFilePath);

			blobHelper.UploadAsync(imagePath, bytes).Wait();

			Assert.IsTrue(true);
		}

		private static void BlobFetchTest()
		{
			var blobHelper = _services.GetService<BlobStorageHelper<BlobStorageOptions>>();

			var imagePath = "Issues/2a5eb2f8-6673-40ba-a2f3-f7cdf259aa3e.png";
			var image = blobHelper.GetBlobBase64ImageAsync(imagePath).Result;

			Assert.IsNotNull(image);
		}

		private static void BlobCopyTest()
		{
			var blobHelper = _services.GetService<BlobStorageHelper<BlobStorageOptions>>();
			var imagePath = "images/e_sig_banner.png";
			var destinationPath = $"archive/{Path.GetFileNameWithoutExtension(imagePath)}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";

			var success = blobHelper.CopyFileAsync(imagePath, destinationPath).Result;

			Assert.IsTrue(success);
		}

		private static ServiceProvider ConfigureServices(IServiceCollection services)
		{
			var builder = new ConfigurationBuilder()
								.SetBasePath(Directory.GetCurrentDirectory())
								.AddJsonFile("appSettings.json", true)
								.AddEnvironmentVariables();

			//Determines the working environment as IHostingEnvironment is unavailable in a console app
			var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
			var isDevelopment = (string.IsNullOrEmpty(devEnvironmentVariable) || string.Compare(devEnvironmentVariable, "development", false) == 0);

			if (isDevelopment)  //	Do this in Dev
			{

			}

			_config = builder.Build();

			//services.Configure<KeyVaultSecrets>(_config.GetSection(nameof(KeyVaultSecrets)))
			//	.AddOptions();

			//services.AddLogging(configure => configure.AddConsole())
			//	.AddTransient<ProcessHelper>();

			services.AddHttpClient();
			services.AddTransient<HttpInjectableHelper>();

			services.AddSingleton<IConfiguration>(services =>
			{
				return _config;
			});

			services.AddOptions<BlobStorageOptions>()
						.Configure<IConfiguration>((settings, configuration) =>
						{
							configuration.GetSection(nameof(BlobStorageOptions)).Bind(settings);
						});

			services.AddTransient<BlobStorageHelper<BlobStorageOptions>>();

			return services.BuildServiceProvider();
		}

		private static string GetSecret(string key)
		{
			try
			{
				var kvh = new KeyVaultHelper("https://azuro-app-keys.vault.azure.net");
				var secret = kvh.GetSecret(key);

				return secret;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				return null;
			}
		}

		public static async Task GetExpiredSecrets()
		{
			var kvh = new KeyVaultHelper("https://azuro-app-keys.vault.azure.net");
			await foreach (var p in kvh.GetExpiredSecretPropertiesAsync())
			{
				Assert.IsTrue(p.ExpiresOn.HasValue);

				Assert.IsTrue(p.ExpiresOn < DateTime.UtcNow);

				Console.WriteLine($"Name: {p.Name}: Expired: {p.ExpiresOn:yyyy/MM/dd}");
			}
		}

		private static void PostTest()
		{
			dynamic input = new { OrgName = "ju-test-importer3", StartDate = new DateTime(2020, 1, 6) };
			_ = HttpRequestHelper.PostJsonAsync<object>("https://azuro-fn-devops.azurewebsites.net/api/DevOps/Tenant/AddUpdate?code=MMbtocZm4FlAzExwMLdGOKlT8PK9MIY2K6lDAyuEFIvSaEOtLMwKvQ==", null, input).Result;
		}

		private static void StringTest()
		{
			var imageType = "DASH";
			var all = false;
			//	Fix testable
			var httpHelper = _services.GetService<HttpInjectableHelper>();

			var test = httpHelper.GetStringAsync($"http://localhost:7071/api/Settings/SiteImages/{imageType}{(all == true ? "/All" : string.Empty)}", null).Result;

			Console.WriteLine(test);
		}
	}

}
