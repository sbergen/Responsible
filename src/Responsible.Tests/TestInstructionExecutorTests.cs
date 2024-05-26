using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class TestInstructionExecutorTests : ResponsibleTestBase
	{
		protected override IReadOnlyList<Type> RethrowableExceptions => new[]
		{
			typeof(SuccessException),
			typeof(IgnoreException),
			typeof(InconclusiveException),
		};

		[Test]
		public async Task Dispose_CancelsInstructions()
		{
			var task = Never.ExpectWithinSeconds(1).ToTask(this.Executor);
			this.Executor.Dispose();
			var exception = await AwaitFailureExceptionForUnity(task);
			exception.InnerException.Should().BeOfType<TaskCanceledException>();
		}

		[Test]
		public void StartingInstruction_Throws_AfterDispose()
		{
			this.Executor.Dispose();
			var task = Return(0).ToTask(this.Executor);
			(task.Exception?.InnerException).Should().BeOfType<ObjectDisposedException>();
		}

		[Test]
		public async Task StartingInstructionTwice_Throws()
		{
			var state = Return(0).CreateState();
			var task1 = state.ToTask(this.Executor);
			var task2 = state.ToTask(this.Executor);

			task1.IsFaulted.Should().BeFalse();
			var exception = await AwaitFailureExceptionForUnity(task2);
			exception.InnerException.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public void RethrowableException_GetsRethrown()
		{
			var task = Do(
					"Call Assert.Ignore",
					() => Assert.Ignore("This test is ignored"))
				.ToTask(this.Executor);
			(task.Exception?.InnerException).Should().BeOfType<IgnoreException>();
		}
	}
}
