using FluentAssertions;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class LinqTests : ResponsibleTestBase
	{
		[Test]
		public void LinqQuery_WorksAsExpected()
		{
			BuildQuery().ToTask(this.Executor).AssertSynchronousResult()
				.Should().Be(10);
		}

		private static ITestInstruction<int> BuildQuery() =>
			from a in Return(2)
			from b in Return(3)
			let c = a + b
			from result in Return(2 * c)
			select result;
	}
}
