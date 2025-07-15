using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Azuro.Common
{
	public enum OSBitVersion
	{
		Unknown,
		x86,
		x64,
	}

	/// <summary>
	/// A collection of common Utility functions. This class should not be inherited as all methods are 
	/// static, and largely unrelated.
	/// </summary>
	public static class Utility
	{
		/// <summary>
		/// Creates an object of Type T.
		/// </summary>
		/// <typeparam name="T">The type of the object to create.</typeparam>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object of type T.</returns>
		public static T CreateObject<T>(params object[] args)
		{
			return (T)CreateObject(typeof(T), args);
		}

		/// <summary>
		/// Create an object of Type T from the supplied assembly, and type parameters.
		/// </summary>
		/// <typeparam name="T">The type of the object to create.</typeparam>
		/// <param name="assembly">The name of the assembly from which to create the object.</param>
		/// <param name="type">The type of object to create.</param>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object of type T.</returns>
		public static T CreateObject<T>(string assembly, string type, params object[] args)
		{
			return (T)CreateObject(Assembly.CreateQualifiedName(assembly, type), args);
		}

		/// <summary>
		/// Create an object of Type T from the supplied assembly, and type parameters.
		/// </summary>
		/// <typeparam name="T">The type of the object to create.</typeparam>
		/// <param name="type">The type of object to create.</param>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object of type T.</returns>
		public static T CreateObject<T>(string type, params object[] args)
		{
			return (T)CreateObject(type, args);
		}

		/// <summary>
		/// Create an object of Type T from the supplied assembly, and type parameters.
		/// </summary>
		/// <typeparam name="T">The type of the object to create.</typeparam>
		/// <param name="type">The type of object to create.</param>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object of type T.</returns>
		public static T CreateObject<T>(Type type, params object[] args)
		{
			return (T)CreateObject(type, args);
		}

		/// <summary>
		/// Create an object from the supplied assembly, and type parameters.
		/// </summary>
		/// <param name="assembly">The name of the assembly from which to create the object.</param>
		/// <param name="type">The type of object to create.</param>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object.</returns>
		public static object CreateObject(string assembly, string type, params object[] args)
		{
			return CreateObject(Assembly.CreateQualifiedName(assembly, type), args);
		}

		/// <summary>
		/// Create an object from the supplied assembly, and type parameters.
		/// </summary>
		/// <param name="type">The type of object to create.</param>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object.</returns>
		public static object CreateObject(string type, params object[] args)
		{
			Type t = Type.GetType(type, true);
			return CreateObject(t, args);
		}

		/// <summary>
		/// Create an object from the supplied assembly, and type parameters.
		/// </summary>
		/// <param name="type">The type of object to create.</param>
		/// <param name="args">Any construction arguments that might be needed by the object.</param>
		/// <returns>An object.</returns>
		public static object CreateObject(Type type, params object[] args)
		{
			return type != null
				? Activator.CreateInstance(type, args)
				: throw new ArgumentNullException($"{nameof(type)}", $"{nameof(CreateObject)} - Type supplied was NULL.");
		}

		/// <summary>
		/// Checks that a DataSet has at least one table and one row.
		/// </summary>
		/// <param name="ds">The <see cref="DataSet">DataSet</see> to check.</param>
		/// <returns>True if valid, else false.</returns>
		public static bool IsValid(DataSet ds)
		{
			return (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0);
		}

		/// <summary>
		/// Checks that a DataSet has at least one table and one row.
		/// </summary>
		/// <param name="dt">The <see cref="DataTable">DataSet</see> to check.</param>
		/// <returns>True if valid, else false.</returns>
		public static bool IsValid(DataTable dt)
		{
			return (dt != null && dt.Rows.Count > 0);
		}

		/// <summary>
		/// Checks that a DateTime? is not null, and a valid value.
		/// </summary>
		/// <param name="dt">The <see cref="DateTime">DateTime</see> to check.</param>
		/// <returns>True if valid, else false.</returns>
		public static bool IsValid(DateTime? dt)
		{
			return dt.HasValue && dt.Value != DateTime.MinValue && dt.Value != DateTime.MaxValue;
		}

		/// <summary>
		/// Safely convert an object to an integer.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>The converted integer value and 0 if it was invalid.</returns>
		/// <remarks>The integer passed in could legitimately be 0, so this method should not be 
		/// used if knowing whether the value was invalid is important.</remarks>
		public static int FieldValueToInt32(object val)
		{
			return FieldValueToInt32(val, 0);
		}

		/// <summary>
		/// Safely convert an object to an integer.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <returns>The converted integer value and 'defaultVal' if it was invalid.</returns>
		/// <remarks>The integer passed in could legitimately be equal to the default, so this 
		/// method should be used carefully if knowing whether the value was invalid is important.</remarks>
		public static int FieldValueToInt32(object val, int defaultVal)
		{
			if (val == null || Convert.IsDBNull(val))
				return defaultVal;
			else if (val is int @int)
				return @int;


			if (int.TryParse(Convert.ToString(val), out int result))
			{
				return result;
			}
			else
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// Safely convert an object to a 64-bit integer.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>The converted 64-bit integer value and 0 if it was invalid.</returns>
		/// <remarks>The integer passed in could legitimately be 0, so this method should not be 
		/// used if knowing whether the value was invalid is important.</remarks>
		public static long FieldValueToInt64(object val)
		{
			return FieldValueToInt64(val, 0);
		}

		/// <summary>
		/// Safely convert an object to a 64-bit integer.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <returns>The converted 64-bit integer value and 'defaultVal' if it was invalid.</returns>
		/// <remarks>The integer passed in could legitimately be equal to the default, so this 
		/// method should be used carefully if knowing whether the value was invalid is important.</remarks>
		public static long FieldValueToInt64(object val, long defaultVal)
		{
			if (val == null || Convert.IsDBNull(val))
				return defaultVal;


			if (long.TryParse(Convert.ToString(val), out long result))
			{
				return result;
			}
			else
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// Safely convert an object to a decimal value.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>The converted decimal value and 0 if it was invalid.</returns>
		/// <remarks>The decimal value passed in could legitimately be 0, so this method should not be 
		/// used if knowing whether the value was invalid is important.</remarks>
		public static decimal FieldValueToDecimal(object val)
		{
			return FieldValueToDecimal(val, 0M);
		}

		/// <summary>
		/// Safely convert an object to a decimal value.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <returns>The converted decimal value and 'defaultVal' if it was invalid.</returns>
		/// <remarks>The decimal value passed in could legitimately be equal to the default, so this 
		/// method should be used carefully if knowing whether the value was invalid is important.</remarks>
		public static decimal FieldValueToDecimal(object val, decimal defaultVal)
		{
			if (val == null || Convert.IsDBNull(val))
				return defaultVal;
			else if (val is decimal @decimal)
				return @decimal;
			else if (val is string @string)
			{
				if (@string.Length == 0)
					return defaultVal;
				else if (!IsNumeric(@string))
					return defaultVal;
				else
				{
					if (decimal.TryParse(Convert.ToString(val), out decimal result))
					{
						return result;
					}
					else
					{
						return defaultVal;
					}
				}
			}
			try
			{
				return Convert.ToDecimal(val);
			}
			catch
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// Safely convert an object to a double value.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>The converted double value and 0 if it was invalid.</returns>
		/// <remarks>The double value passed in could legitimately be 0, so this method should not be 
		/// used if knowing whether the value was invalid is important.</remarks>
		public static double FieldValueToDouble(object val)
		{
			return FieldValueToDouble(val, 0.0);
		}

		/// <summary>
		/// Safely convert an object to a double value.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <returns>The converted double value and 'defaultVal' if it was invalid.</returns>
		/// <remarks>The double value passed in could legitimately be equal to the default, so this 
		/// method should be used carefully if knowing whether the value was invalid is important.</remarks>
		public static double FieldValueToDouble(object val, double defaultVal)
		{
			if (val == null || Convert.IsDBNull(val))
				return defaultVal;
			else if (val is double @double)
				return @double;
			else if (val is string @string)
			{
				if (@string.Length == 0)
					return defaultVal;
				else
				{
					if (!IsNumeric(@string))
						return defaultVal;
				}
			}

			if (double.TryParse(Convert.ToString(val), out double result))
			{
				return result;
			}
			else
			{
				return defaultVal;
			}
		}

		/// <summary>
		/// Safely convert an object to a DateTime value.
		/// </summary>
		/// <param name="date">The value to convert.</param>
		/// <returns>The converted DateTime value and DateTime.MinValue if it was invalid.</returns>
		/// <remarks>The DateTime value passed in could legitimately be equal to DateTime.MinValue, so this 
		/// method should not be used if knowing whether the value was invalid is important.</remarks>
		public static DateTime FieldValueToDateTime(object date)
		{
			return FieldValueToDateTime(date, DateTime.MinValue, null);
		}

		/// <summary>
		/// Safely convert an object to a DateTime value.
		/// </summary>
		/// <param name="date">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <returns>The converted DateTime value and 'defaultVal' if it was invalid.</returns>
		/// <remarks>The DateTime value passed in could legitimately be equal to the default, so this 
		/// method should be used carefully if knowing whether the value was invalid is important.</remarks>
		public static DateTime FieldValueToDateTime(object date, DateTime defaultVal)
		{
			return FieldValueToDateTime(date, defaultVal, null);
		}

		/// <summary>
		/// Safely convert an object to a DateTime value.
		/// </summary>
		/// <param name="date">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <param name="parseMask">The expected string format for the date</param>
		/// <returns>The converted DateTime value and 'defaultVal' if it was invalid.</returns>
		/// <remarks>The DateTime value passed in could legitimately be equal to the default, so this 
		/// method should be used carefully if knowing whether the value was invalid is important.</remarks>
		public static DateTime FieldValueToDateTime(object date, DateTime defaultVal, string parseMask)
		{
			if (date == null || Convert.IsDBNull(date))
				return defaultVal;
			else if (date is DateTime time)
				return time;
			else if (date is string)
			{
				DateTime dt;
				if (string.IsNullOrEmpty(parseMask))
				{
					if (DateTime.TryParse(date.ToString(), out dt))
						return dt;
				}
				else if (DateTime.TryParseExact(date.ToString(), parseMask, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.NoCurrentDateDefault, out dt))
					return dt;
			}
			return defaultVal;
		}

		/// <summary>
		/// Safely convert an object to a Boolean value.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>The converted Boolean value and false if it was invalid.</returns>
		public static bool FieldValueToBoolean(object val)
		{
			return FieldValueToBoolean(val, false);
		}

		/// <summary>
		/// Safely convert an object to a Boolean value.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <param name="defaultVal">The default value to return when invalid.</param>
		/// <returns>The converted Boolean value and 'defaultVal' if it was invalid.</returns>
		public static bool FieldValueToBoolean(object val, bool defaultVal)
		{
			return Convert.IsDBNull(val) ? defaultVal : (val is bool) ? Convert.ToBoolean(val) : defaultVal;
		}

		/// <summary>
		/// Converts a value to a string. Will handle arrays and byte arrays.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>A string representation of the passed in value.</returns>
		public static string FieldValueToString(object val)
		{
			if (val is Array valArr)
			{
				if (valArr.GetValue(0) is byte)
					return new System.Text.ASCIIEncoding().GetString((byte[])val);
				else
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					for (int i = 0; i < valArr.Length; ++i)
						sb.Append(valArr.GetValue(i));
					return sb.ToString();
				}
			}
			return val.ToString();
		}

		/// <summary>
		/// A generic method for safely changing from one type to another.
		/// </summary>
		/// <param name="value">The value that must be changed to a new type.</param>
		/// <param name="type">The type that the value is to be changed to.</param>
		/// <returns>The value as the new type.</returns>
		public static object ChangeType(object value, Type type)
		{
			if (value == null)
				if (type.IsGenericType)
					return Activator.CreateInstance(type);
				else
					return null;
			if (type == value.GetType())
				return value;
			if (type.IsEnum)
			{
				if (value is string)
					return Enum.Parse(type, value as string);
				else
					return Enum.ToObject(type, value);
			}
			if (!type.IsInterface && type.IsGenericType)
			{
				Type innerType = type.GetGenericArguments()[0];
				object innerValue = ChangeType(value, innerType);
				return Activator.CreateInstance(type, new object[] { innerValue });
			}
			if (value is string && type == typeof(Guid))
				return new Guid(value as string);
			if (value is string && type == typeof(Version))
				return new Version(value as string);
			if (!(value is IConvertible))
				return value;
			return Convert.ChangeType(value, type);
		}

		/// <summary>
		/// Checks a string to see whether its numeric.
		/// </summary>
		/// <param name="val">The string to check.</param>
		/// <returns>True if it's a valid numeric, else false.</returns>
		/// <remarks>This method uses <see cref="NumberStyles">NumberStyles.Currency</see> and 
		/// <see cref="CultureInfo">CultureInfo.CurrentCulture</see> by default.</remarks>
		public static bool IsNumeric(string val)
		{
			return IsNumeric(val, NumberStyles.Currency, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Checks a string to see whether its numeric.
		/// </summary>
		/// <param name="val">The string to check.</param>
		/// <param name="style">The number styles to use when checking for numeric validity.</param>
		/// <returns>True if it's a valid numeric, else false.</returns>
		/// <remarks>This method uses <see cref="CultureInfo">CultureInfo.CurrentCulture</see> 
		/// by default.</remarks>
		public static bool IsNumeric(string val, NumberStyles style)
		{
			return IsNumeric(val, style, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Checks a type for numeric-ness.
		/// </summary>
		/// <param name="t">The type to check.</param>
		/// <returns>True if it's a valid numeric, else false.</returns>
		public static bool IsNumeric(Type t)
		{
			if ((t.IsArray))
			{
				return false;
			}
			switch (Type.GetTypeCode(t))
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks a string to see whether its numeric.
		/// </summary>
		/// <param name="val">The string to check.</param>
		/// <param name="style">The number styles to use when checking for numeric validity.</param>
		/// <param name="format">A format provider for Culture specific checks.</param>
		/// <returns>True if its a valid numeric, else false.</returns>
		public static bool IsNumeric(string val, NumberStyles style, IFormatProvider format)
		{
			return decimal.TryParse(val, style, format, out _);
		}

		/// <summary>
		/// A helper method to set a column of a row in a data table to a specific value,
		/// based on the column's type.
		/// </summary>
		/// <param name="dr">The row to set.</param>
		/// <param name="columnName">The name of the column to set.</param>
		/// <param name="newValue">The value to set the column to.</param>
		public static void SetColumnValue(DataRow dr, string columnName, object newValue)
		{
			dr[columnName] = newValue is string @string && string.IsNullOrEmpty(@string)
				? DBNull.Value
				: Convert.ChangeType(newValue, dr.Table.Columns[columnName].DataType);
		}

		/// <summary>
		/// A helper method to unpack an exception recursively into the inner exceptions. For more customisable
		/// Exception Manager see the Azuro Exception Management library.
		/// </summary>
		/// <param name="ex">The Exception to unpack.</param>
		/// <returns>A string containing the exception and all inner exceptions.</returns>
		public static string UnpackException(Exception ex)
		{
			var msg = $"{ex.Source} -> {ex.Message} ({ex.GetType()} | {ex.HResult}){Environment.NewLine}\t{ex.StackTrace}";
			if (ex?.InnerException != null)
				msg = $"{Environment.NewLine}Inner Exception{UnpackException(ex.InnerException)}";

			return msg;
		}

		/// <summary>
		/// This method is used to evaluate an enum with the Flags attribute set and returns true if 
		/// the evaluation is valid.
		/// </summary>
		/// <param name="flagsEnum">The enumeration value to check.</param>
		/// <param name="value">The flags to check for.</param>
		/// <returns>True if it contains the flag, else false.</returns>
		public static bool CheckFlag(ulong flagsEnum, ulong value)
		{
			return CheckFlag(flagsEnum, value, false);
		}

		/// <summary>
		/// This method is used to evaluate an enum with the Flags attribute set and returns true if 
		/// the evaluation is valid.
		/// </summary>
		/// <param name="flagsEnum">The enumeration value to check.</param>
		/// <param name="value">The flags to check for.</param>
		/// <param name="strict">When set, will enforce that the flags match exactly.</param>
		/// <returns>True if it contains the flag, else false.</returns>
		public static bool CheckFlag(ulong flagsEnum, ulong value, bool strict)
		{
			//return (strict ? (flagsEnum & value) == 0 : (flagsEnum & value) > 0);
			return strict ? (flagsEnum & value) == value : (flagsEnum & value) != 0;
		}

		//	TODO: Place in separate class
		#region Password Generator

		//	TODO: Use static strings
		//	static readonly string Upper = "ABCDE..";
		//	static readonly string Lower = "abcde..";
		//	etc
		private static readonly char[] pwdChars = {
											   '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '!', '@', '#', '$', 'a', 'b',
											   'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r',
											   's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
											   'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
											   'Y', 'Z'
										   };

		[Flags]
		public enum PasswordAttributes
		{
			Letters = 1,
			Digits = 2,
			SpecialChars = 4,
			All = Letters | Digits | SpecialChars,
			LettersAndDigits = Letters | Digits,
			LettersAndSpecialChars = Letters | SpecialChars,
			DigitsAndSpecialChars = Digits | SpecialChars,
		}

		public static string GeneratePassword(int length)
		{
			return GeneratePassword(length, PasswordAttributes.All);
		}

		public static string GeneratePassword(int length, PasswordAttributes passwordAttributes)
		{
			StringBuilder pwd = new StringBuilder(length);
			do
			{
				while (pwd.Length < length)
				{
					char c = pwdChars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(pwdChars.Length)];
					if (!IsValidPasswordChar(c, passwordAttributes))
						continue;

					pwd.Append(c);
				}
			} while (!PasswordIsValid(pwd.ToString(), passwordAttributes));

			return pwd.ToString();
		}

		private static bool IsValidPasswordChar(char c, PasswordAttributes passwordAttributes)
		{
			switch (passwordAttributes)
			{
				case PasswordAttributes.Digits:
					return char.IsDigit(c);
				case PasswordAttributes.Letters:
					return char.IsLetter(c);
				case PasswordAttributes.SpecialChars:
					return char.IsSymbol(c) || char.IsPunctuation(c);
				case PasswordAttributes.LettersAndDigits:
					return char.IsLetterOrDigit(c);
				case PasswordAttributes.LettersAndSpecialChars:
					return char.IsLetter(c) || char.IsSymbol(c) || char.IsPunctuation(c);
				case PasswordAttributes.DigitsAndSpecialChars:
					return char.IsDigit(c) || char.IsSymbol(c) || char.IsPunctuation(c);
				case PasswordAttributes.All:
				default:
					break;
			}

			return true;
		}

		private static bool PasswordIsValid(string password, PasswordAttributes passwordAttributes)
		{
			bool valid = true;
			switch (passwordAttributes)
			{
				case PasswordAttributes.Digits:
					{
						foreach (char c in password)
							valid &= char.IsDigit(c);
						break;
					}
				case PasswordAttributes.Letters:
					{
						foreach (char c in password)
							valid &= char.IsLetter(c);
						break;
					}
				case PasswordAttributes.SpecialChars:
					{
						foreach (char c in password)
							valid &= char.IsSymbol(c) || char.IsPunctuation(c);
						break;
					}
				case PasswordAttributes.LettersAndDigits:
					{
						foreach (char c in password)
							valid &= char.IsLetterOrDigit(c);
						break;
					}
				case PasswordAttributes.LettersAndSpecialChars:
					{
						bool validLetter = false, validSymbol = false;
						foreach (char c in password)
						{
							validLetter |= char.IsLetter(c);
							validSymbol |= char.IsSymbol(c) || char.IsPunctuation(c);
						}
						valid = validLetter && validSymbol;
						break;
					}
				case PasswordAttributes.DigitsAndSpecialChars:
					{
						bool validDigit = false, validSymbol = false;
						foreach (char c in password)
						{
							validDigit |= char.IsDigit(c);
							validSymbol |= char.IsSymbol(c) || char.IsPunctuation(c);
						}
						valid = validDigit && validSymbol;
						break;
					}
				case PasswordAttributes.All:
				default:
					{
						bool validLetterDigit = false, validSymbol = false;
						foreach (char c in password)
						{
							validLetterDigit |= char.IsLetterOrDigit(c);
							validSymbol |= char.IsSymbol(c) || char.IsPunctuation(c);
						}
						valid = validLetterDigit && validSymbol;
						break;
					}
			}

			return valid;
		}

		#endregion

		public static OSBitVersion CheckOSVersion
		{
			get
			{
				//	Simplified as per SonarCloud recommendation
				if (IntPtr.Size == 8)
					return OSBitVersion.x64;
				return IntPtr.Size == 4 ? OSBitVersion.x86 : OSBitVersion.Unknown;
			}
		}

		/// <summary>
		/// Creates a single hash from a multiple items
		/// </summary>
		/// <param name="prms">The PRMS.</param>
		/// <returns></returns>
		public static int Hash(params object[] prms)
		{
			return Hash((from p in prms where p != null select p.GetHashCode()).ToArray());
		}

		/// <summary>
		/// Combines multiple hash codes into onex
		/// </summary>
		/// <param name="hashes">The hashes.</param>
		/// <returns></returns>
		public static int Hash(params int[] hashes)
		{
			return hashes.Aggregate((a, b) => (((a << 5) + a) ^ b));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="handles"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static bool WaitAllHandles(IEnumerable<WaitHandle> handles, int timeout)
		{
			if (handles == null)
				return true;

			if (handles.Count() > 64)
			{
				int handleCount = 0;
				bool retVal = true;
				var handles64 = new WaitHandle[64];
				while (handleCount < handles.Count() && retVal)
				{
					for (int i = 0; i < 64 && i + handleCount < handles.Count(); ++i)
						handles64[i] = handles.ElementAt(i + handleCount);
					handleCount += 64;
					retVal = WaitHandle.WaitAll(handles64, timeout, false);
				}
				return retVal;
			}
			else
			{
				//	true if empty, or wait on all handles.
				return !handles.Any() || WaitHandle.WaitAll(handles.ToArray(), timeout, false);
			}
		}

		public static Type SafeTypeLoad(string typeName, string source)
		{
			if (string.IsNullOrEmpty(typeName))
				throw new ArgumentNullException($"{nameof(typeName)}", $"The typeName parameter for [{source}] was NULL.");

			var t = Type.GetType(typeName);
			if (t == null)
				throw new ArgumentNullException($"{nameof(t)}", $"Loading the type [{typeName}] from [{source}] returned NULL.");

			return t;
		}
	}
}