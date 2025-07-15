using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace Azuro.Common.KeyVault
{
	public class UserSecretHelper<T> : ISecretHelper where T : class, new()
	{
		private readonly T _config;
		private T Settings => _config;

		public Func<string, string> KeyShaper { get; set; }

		public UserSecretHelper(IOptions<T> options)
		{
			_config = options.Value;
		}

		public string GetSecret(string key)
		{
			if (KeyShaper != null)
				key = KeyShaper(key);
			var prop = typeof(T).GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			return prop?.GetValue(_config)?.ToString();
		}

		public IAsyncEnumerable<SecretProperties> GetExpiredSecretPropertiesAsync()
		{
			throw new NotImplementedException();
		}
	}
}
