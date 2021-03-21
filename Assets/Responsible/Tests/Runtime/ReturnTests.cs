using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
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
