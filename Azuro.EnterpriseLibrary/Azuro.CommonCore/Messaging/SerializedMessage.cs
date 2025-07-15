using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Azuro.Common.Messaging
{
	[Serializable]
	public abstract class SerializedMessage<T>
	{
		private T TSerializedMessage { get; set; }

		[NonSerialized]
		private System.Text.Encoding _encoding = System.Text.Encoding.Default;

		[XmlIgnore]
		public System.Text.Encoding Encoding
		{
			get { return _encoding; }
			set { _encoding = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializedMessage{T}"/> class.
		/// </summary>
		protected SerializedMessage() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializedMessage{T}"/> class.
		/// </summary>
		/// <param name="otherT">The other t.</param>
		protected SerializedMessage(T otherT)
		{
			TSerializedMessage = otherT;
		}

		/// <summary>
		/// Deserializes the specified in stream.
		/// </summary>
		/// <param name="inStream">The in stream.</param>
		/// <returns></returns>
		public static T Deserialize(Stream inStream)
		{
			return Deserialize(new StreamReader(inStream), null);
		}

		/// <summary>
		/// Deserializes the specified in stream.
		/// </summary>
		/// <param name="inStream">The in stream.</param>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns></returns>
		public static T Deserialize(Stream inStream, Type[] extraTypes)
		{
			return Deserialize(new StreamReader(inStream), extraTypes);
		}

		/// <summary>
		/// Deserializes the specified in node.
		/// </summary>
		/// <param name="inNode">The in node.</param>
		/// <returns></returns>
		public static T Deserialize(XmlNode inNode)
		{
			return Deserialize(new StringReader(inNode.OuterXml), null);
		}

		/// <summary>
		/// Deserializes the specified in node.
		/// </summary>
		/// <param name="inNode">The in node.</param>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns></returns>
		public static T Deserialize(XmlNode inNode, Type[] extraTypes)
		{
			return Deserialize(new StringReader(inNode.OuterXml), extraTypes);
		}

		/// <summary>
		/// Deserializes the specified in string.
		/// </summary>
		/// <param name="inString">The xml document to deserialize as a string.</param>
		/// <returns></returns>
		public static T Deserialize(string inString)
		{
			return Deserialize(new StringReader(inString.Trim()), null);
		}

		/// <summary>
		/// Deserializes the specified in string.
		/// </summary>
		/// <param name="inString">The xml document to deserialize as a string.</param>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns></returns>
		public static T Deserialize(string inString, Type[] extraTypes)
		{
			return Deserialize(new StringReader(inString.Trim()), extraTypes);
		}

		/// <summary>
		/// Deserializes the specified in XML reader.
		/// </summary>
		/// <param name="inXmlReader">The in XML reader.</param>
		/// <returns></returns>
		public static T Deserialize(XmlReader inXmlReader)
		{
			return Deserialize(inXmlReader, null);
		}

		/// <summary>
		/// Deserializes the specified in XML reader.
		/// </summary>
		/// <param name="inXmlReader">The in XML reader.</param>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns></returns>
		public static T Deserialize(XmlReader inXmlReader, Type[] extraTypes)
		{
			if (inXmlReader.Read())
				return Deserialize(new StringReader(inXmlReader.ReadOuterXml()), extraTypes);
			else
				return Activator.CreateInstance<T>();
		}

		/// <summary>
		/// Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public static T Deserialize(TextReader tr)
		{
			return Deserialize(tr, null);
		}

		/// <summary>
		/// Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns></returns>
		public static T Deserialize(TextReader tr, Type[] extraTypes)
		{
			XmlSerializer xs = null;
			if (extraTypes == null)
				xs = new XmlSerializer(typeof(T));
			else
				xs = new XmlSerializer(typeof(T), extraTypes);

			return (T)xs.Deserialize(tr);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return CustomToString(null);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public string ToString(Type[] extraTypes)
		{
			return CustomToString(extraTypes);
		}

		/// <summary>
		/// Customs to string.
		/// </summary>
		/// <param name="extraTypes">The extra types.</param>
		/// <returns></returns>
		private string CustomToString(Type[] extraTypes)
		{
			//ExtraTypes have to contains something, not allowed to be null
			XmlSerializer xs = (extraTypes == null ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes));
			using (MemoryStream ms = new MemoryStream())
			{
				using (XmlTextWriter xw = new XmlTextWriter(ms, Encoding))
				{
					xs.Serialize(xw, (object)TSerializedMessage ?? this);
					return Encoding.GetString(ms.ToArray()).Trim();
				}
			}
		}

		/// <summary>
		/// To the XML.
		/// </summary>
		/// <returns></returns>
		public XmlNode ToXml()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(ToString());
			return doc.DocumentElement;
		}
	}
}