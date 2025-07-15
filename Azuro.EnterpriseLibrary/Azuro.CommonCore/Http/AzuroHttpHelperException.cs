using System;
using System.Runtime.Serialization;

namespace Azuro.Common.Http
{
	/// <summary>
	/// Custom exception for HttpInjectableHelper exceptions
	/// </summary>
	[Serializable]
	public class AzuroHttpHelperException : Exception
	{
		public AzuroHttpHelperException(string message) : base(message) { }
		public AzuroHttpHelperException(string message, Exception ex) : base(message, ex) { }
		protected AzuroHttpHelperException(SerializationInfo info, StreamingContext context)
				  : base(info, context)
		{
		}
	}
}
