using System;
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
				new[] { typeof(IgnoreException) });
		}

		[TearDown]
		public void BaseTearDown()
		{
			this.Executor.Dispose();
		}

		protected virtual IExternalResultSource ExternalResultSource() => null;
		protected virtual IFailureListener MakeFailureListener() => null;
		protected virtual IGlobalContextProvider MakeGlobalContextProvider() => null;
	}
}
