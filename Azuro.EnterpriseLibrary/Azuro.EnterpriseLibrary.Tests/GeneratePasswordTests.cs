using System.Text.RegularExpressions;
using Azuro.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azuro.EnterpriseLibrary.Tests
{
	//	TODO: Complete coverage
	[TestClass]
	public class GeneratePasswordTests
	{
		[TestMethod]
		public void TestValidPasswordLettersOnly()
		{
			const int PWD_LENGTH = 10;
			var pwd = Utility.GeneratePassword(PWD_LENGTH, Utility.PasswordAttributes.Letters);
			StringAssert.Matches(pwd, new Regex($"[a-zA-Z]{{{PWD_LENGTH},{PWD_LENGTH}}}"));
		}
	}
}
