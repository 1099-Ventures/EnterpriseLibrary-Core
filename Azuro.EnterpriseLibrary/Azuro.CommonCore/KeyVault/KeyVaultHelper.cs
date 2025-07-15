using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azuro.Common.KeyVault
{
	public class KeyVaultHelper : ISecretHelper
	{
		private readonly ILogger _logger;
#pragma warning disable CS0618 // Type or member is obsolete
		private string VaultUrl => String.IsNullOrEmpty(_config.VaultName) ? _config?.VaultUrl : $"https://{_config.VaultName}.vault.azure.net/";
#pragma warning restore CS0618 // Type or member is obsolete
		private readonly KeyVaultConfig _config;

		public KeyVaultHelper() { }
		public KeyVaultHelper(string vaultUrl)
		{
			//	TODO: Update this constructor to extract vaultname 
#pragma warning disable CS0618 // Type or member is obsolete
			//	Assuming this is here for the test methods
			_config = new KeyVaultConfig { VaultUrl = vaultUrl, AllowInteractiveLogin = true };
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public KeyVaultHelper(ILogger<KeyVaultHelper> logger)
		{
			_logger = logger;
		}

		public KeyVaultHelper(ILogger<KeyVaultHelper> logger, IOptions<KeyVaultConfig> options)
		{
			_logger = logger;
			_config = options.Value;
#pragma warning disable CS0618 // Type or member is obsolete
			LogTrace($"Url: {_config.VaultUrl}");
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public string GetSecret(string key)
		{
			SecretClient kvc = CreateKeyVaultClient();
			LogTrace($"Retrieving secret [{key}] from [{VaultUrl}]");
			var secret = kvc.GetSecret(key);
			return secret.Value.Value;
		}

		public async IAsyncEnumerable<SecretProperties> GetExpiredSecretPropertiesAsync()
		{
			SecretClient kvc = CreateKeyVaultClient();

			await foreach (var p in kvc.GetPropertiesOfSecretsAsync())
			{
				if (p.Enabled == true && p.ExpiresOn.HasValue && p.ExpiresOn < DateTime.UtcNow)
				{
					yield return new SecretProperties
					{
						Name = p.Name,
						ExpiresOn = p.ExpiresOn,
						CreatedOn = p.CreatedOn,
						Enabled = p.Enabled == true,
					};
				}
			}
		}

		private SecretClient CreateKeyVaultClient() => new(new Uri(VaultUrl), new DefaultAzureCredential(_config.AllowInteractiveLogin));

		[SuppressMessage("Microsoft.Design", "IDE0051")]
		private void LogError(string message)
		{
			_logger?.LogError(message);
		}

		private void LogTrace(string message)
		{
			_logger?.LogTrace(message);
		}
	}
}
