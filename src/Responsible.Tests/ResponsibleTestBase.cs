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

		protected IFailureListener? FailureListener { get; private set; }
		protected IGlobalContextProvider? GlobalContextProvider { get; private set; }
		protected MockTestScheduler Scheduler { get; private set; } = null!;

		protected TestInstructionExecutor Executor { get; private set; } = null!;

		protected static void Nop<T>(T unused)
		{
		}

		protected void AdvanceDefaultFrame() => this.Scheduler.AdvanceFrame(OneFrame);

		protected static TestFailureException GetFailureException(Task task)
		{
			Assert.IsNotNull(task.Exception, "Should have exception");
			var exception = task.Exception!;
			Assert.AreEqual(1, exception.InnerExceptions.Count);
			Assert.IsInstanceOf<TestFailureException>(exception.InnerExceptions[0]);
			return (TestFailureException)exception.InnerExceptions[0];
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

		protected virtual IExternalResultSource? ExternalResultSource() => null;
		protected virtual IFailureListener? MakeFailureListener() => null;
		protected virtual IGlobalContextProvider? MakeGlobalContextProvider() => null;
		protected virtual IReadOnlyList<Type>? RethrowableExceptions => null;
	}
}
