using System.Collections.Generic;

namespace Azuro.Common.KeyVault
{
	public interface ISecretHelper
	{
		string GetSecret(string key);
		IAsyncEnumerable<SecretProperties> GetExpiredSecretPropertiesAsync();
	}
}
