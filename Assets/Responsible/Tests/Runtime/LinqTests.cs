using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class LinqTests : ResponsibleTestBase
	{
		[Test]
		public void LinqQuery_WorksAsExpected()
		{
			Assert.AreEqual(
				10,
				BuildQuery().ToTask(this.Executor).AssertSynchronousResult());
		}

		private static ITestInstruction<int> BuildQuery() =>
			from a in Return(2)
			from b in Return(3)
			let c = a + b
			from result in Return(2 * c)
			select result;
	}
}
