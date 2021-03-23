using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class OperationStateTests : ResponsibleTestBase
	{
		[Test]
		public void ReusingSameInstruction_ProvidesSeparateState()
		{
			bool condition = true;

			bool WaitForIt()
			{
				if (condition)
				{
					condition = false;
					return true;
				}

				return condition;
			}

			var wait = WaitForCondition("Wait for it", WaitForIt);

			var task = wait.AndThen(wait)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.TimeProvider.AdvanceFrame(OneSecond);

			var exception = GetAssertionException(task);
			Assert.That(
				exception.Message,
				Does.Contain("[-] Wait for it").And.Contain("[✓] Wait for it"));
		}

	}
}
