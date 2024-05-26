using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Responsible.Tests
{
	/// <summary>
	/// Tests to check how tasks behave in the runtime.
	/// Unity 2021.3 seems to have a bug (?), which causes task cancellation to be asynchronous.
	/// </summary>
	public class TaskBehaviorTests
	{
		[Test]
		public void TaskException_IsHandledSynchronously()
		{
			var task = FaultTaskAfterCancellation();

#if UNITY_2021_3 || UNITY_2022_3
			Assert.IsNull(
				task.Exception,
				"Expecting Unity 2021 && 2022 to still be broken: If this test fails, remove the workarounds 🎉");
#else
			task.Exception.Should().BeOfType<AggregateException>();
#endif
		}

		[Test]
		public void TaskException_IsHandledSynchronously_WithSuppressedFlow()
		{
			using (ExecutionContext.SuppressFlow())
			{
				var task = FaultTaskAfterCancellation();
				task.Exception.Should().BeOfType<AggregateException>();
			}
		}

		private static Task FaultTaskAfterCancellation()
		{
			var cts = new CancellationTokenSource();
			var tcs = new TaskCompletionSource<int>(cts);
			cts.Token.Register(tcs.SetCanceled);

			var task = AwaitAndThrowDifferentException(tcs.Task);

			cts.Cancel();

			return task;
		}

		private static async Task AwaitAndThrowDifferentException(Task task)
		{
			try
			{
				await task;
			}
			catch
			{
				throw new Exception();
			}
		}
	}
}
