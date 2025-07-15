using System;

namespace Azuro.Common.KeyVault
{
	public class SecretProperties
	{
		public string Name { get; set; }
		public DateTimeOffset? ExpiresOn { get; set; }
		public DateTimeOffset? CreatedOn { get; set; }
		public bool Enabled { get; set; }
	}
}
