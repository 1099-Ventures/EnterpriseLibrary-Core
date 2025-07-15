using System;
using System.Runtime.Serialization;

namespace Azuro.Common.AzureStorage
{
	/// <summary>
	/// Specialized Exception to make it easier for callers to identify that the source of an exception is our custom code.
	/// </summary>
	[Serializable]
	public class AzuroStorageException : Exception
	{
		public AzuroStorageException(string message) : base(message) { }
		public AzuroStorageException(string message, Exception ex) : base(message, ex) { }
		protected AzuroStorageException(SerializationInfo info, StreamingContext context)
				  : base(info, context)
		{
		}
	}
}
