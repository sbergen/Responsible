using System;
using System.Threading.Tasks;
using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class TestInstructionExecutorTests : ResponsibleTestBase
	{
		[Test]
		public void Dispose_CancelsInstructions()
		{
			var task = Never.ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Executor.Dispose();
			var exception = GetFailureException(task);
			Assert.IsInstanceOf<TaskCanceledException>(exception.InnerException);
		}

		[Test]
		public void StartingInstruction_Throws_AfterDispose()
		{
			this.Executor.Dispose();
			var task = Return(0).ToTask(this.Executor);
			Assert.IsInstanceOf<ObjectDisposedException>(task.Exception?.InnerException);
		}

	}
}
