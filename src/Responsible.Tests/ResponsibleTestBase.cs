using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ResponsibleTestBase
	{
		protected static readonly ITestWaitCondition<bool> ImmediateTrue =
			WaitForConditionOn("True", () => true, val => val);

		protected static readonly ITestWaitCondition<bool> Never =
			WaitForConditionOn("Never", () => false, _ => false);

		protected static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
		protected static readonly TimeSpan OneFrame = TimeSpan.FromSeconds(1.0 / 60);

		protected IFailureListener FailureListener { get; private set; }
		protected IGlobalContextProvider GlobalContextProvider { get; private set; }
		protected MockTestScheduler Scheduler { get; private set; }

		protected TestInstructionExecutor Executor { get; private set; }

		protected static void Nop<T>(T unused)
		{
		}

		protected void AdvanceDefaultFrame() => this.Scheduler.AdvanceFrame(OneFrame);

		protected static TestFailureException GetFailureException(Task task)
		{
			Assert.IsNotNull(task.Exception, "Should have exception");
			Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
			Assert.IsInstanceOf<TestFailureException>(task.Exception.InnerExceptions[0]);
			return task.Exception.InnerExceptions[0] as TestFailureException;
		}

		// At least cancellation completes asynchronously (but later in the frame) in Unity 2021
		protected static async Task<TestFailureException> AwaitFailureExceptionForUnity(Task task)
		{
			try
			{
				await AwaitTaskCompletionForUnity(task); // Ensure we have a timeout
				throw new InvalidOperationException("Task was expected to fail, but did not.");
			}
			catch (TestFailureException e)
			{
				return e;
			}
		}

		// Some tasks don't complete asynchronously (but later in the frame) in Unity 2021
		protected static async Task AwaitTaskCompletionForUnity(Task task)
		{
			if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(1))) == task)
			{
#if UNITY_2021_3_OR_NEWER
				var startFrame = UnityEngine.Time.frameCount;
#endif
				await task;
#if UNITY_2021_3_OR_NEWER
				Assert.AreEqual(
					startFrame,
					UnityEngine.Time.frameCount,
					"Task is expected to complete later in the the same frame");
#endif
			}
			else
			{
				throw new TimeoutException("Task did not complete when expected");
			}
		}

		[SetUp]
		public void BaseSetUp()
		{
			this.FailureListener = this.MakeFailureListener();
			this.GlobalContextProvider = this.MakeGlobalContextProvider();
			this.Scheduler = new MockTestScheduler();
			this.Executor = new TestInstructionExecutor(
				this.Scheduler,
				this.ExternalResultSource(),
				this.FailureListener,
				this.GlobalContextProvider,
				this.RethrowableExceptions);
		}

		[TearDown]
		public void BaseTearDown()
		{
			this.Executor.Dispose();
		}

		protected virtual IExternalResultSource ExternalResultSource() => null;
		protected virtual IFailureListener MakeFailureListener() => null;
		protected virtual IGlobalContextProvider MakeGlobalContextProvider() => null;
		protected virtual IReadOnlyList<Type> RethrowableExceptions => null;
	}
}
