using Azuro.Common.KeyVault;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Azuro.EnterpriseLibrary.Tests
{
	[TestClass]
	public class KeyVaultHelperTests
	{
		//	TODO: This test will not reliably pass
		//[TestMethod]
		//public void TestKVH()
		//{
		//	var kvh = new KeyVaultHelper("https://azuro-app-keys.vault.azure.net");
		//	var secret = kvh.GetSecret("devops-azuro-pat");
		//	Assert.IsNotNull(secret);
		//}

		//	TODO: Unable to access keyvault. Cannot login in automated tests.
		//[TestMethod]
		//public async Task TestKVH()
		//{
		//	var kvh = new KeyVaultHelper("https://azuro-app-keys.vault.azure.net");
		//	await foreach (var p in kvh.GetExpiredSecretPropertiesAsync())
		//	{
		//		Assert.IsTrue(p.ExpiresOn.HasValue);

		//		Assert.IsTrue(p.ExpiresOn < DateTime.UtcNow);

		//		Console.WriteLine($"Name: {p.Name}: Expired: {p.ExpiresOn:yyyy/MM/dd}");
		//	}
		//}
	}
}
