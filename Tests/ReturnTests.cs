using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
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

		[Test]
		public void ToString_RetrunsCorrectValue()
		{
			Assert.AreEqual(
				"[ ] Return '42'",
				Return(42).CreateState().ToString());
		}
	}
}
