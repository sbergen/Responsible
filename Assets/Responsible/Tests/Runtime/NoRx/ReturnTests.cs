using NUnit.Framework;
using Responsible.NoRx;
using Responsible.Tests.Runtime.NoRx.Utilities;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
{
	public class ReturnTests : ResponsibleTestBase
	{
		[Test]
		public void Return_ReturnsCorrectValue()
		{
			var value = 42;
			var returnInstruction = Return(value);
			Assert.AreEqual(
				42,
				returnInstruction
					.ToTask(this.Executor)
					.AssertSynchronousResult());
		}
	}
}
