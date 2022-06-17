using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class OperationStateTests : ResponsibleTestBase
	{
		[Test]
		public async Task ReusingSameInstruction_ProvidesSeparateState()
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

			var description = "Wait for it";
			var wait = WaitForCondition(description, WaitForIt);

			var task = wait.AndThen(wait)
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);

			this.Scheduler.AdvanceFrame(OneSecond);

			var exception = await AwaitFailureExceptionForUnity(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("EXPECT WITHIN")
				.Completed(description)
				.JustCanceled(description);
		}

	}
}
