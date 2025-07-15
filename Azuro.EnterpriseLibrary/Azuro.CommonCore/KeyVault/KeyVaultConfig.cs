using System;

namespace Azuro.Common.KeyVault
{
	public class KeyVaultConfig
	{
		[Obsolete("Will be deprecated in a future version of the Enterprise Library. Use VaultName instead")]
		public string VaultUrl { get; set; }
		public string VaultName { get; set; }
		public bool AllowInteractiveLogin { get; set; }
	}
}
