using FluentAssertions;
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
			Return(42)
				.ToTask(this.Executor)
				.AssertSynchronousResult()
				.Should().Be(42);
		}

		[Test]
		public void ToString_RetrunsCorrectValue()
		{
			Return(42).CreateState().ToString()
				.Should().Be("[ ] Return '42'");
		}
	}
}
