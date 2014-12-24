using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telerik.JustMock.EntityFramework.Tests
{
	internal class AssertEx
	{
		public static Exception Throws<T>(Action action) where T : Exception
		{
			try
			{
				action();
			}
			catch (T ex)
			{
				// Test pass
				return ex;
			}
			catch (Exception ex)
			{
				Assert.Fail(String.Format("Wrong exception type thrown. Expected {0}, got {1}.", typeof(T), ex.GetType()));
			}

			Assert.Fail(String.Format("No Expected {0} was thrown", typeof(T).FullName));
			throw new Exception();
		}
	}
}
