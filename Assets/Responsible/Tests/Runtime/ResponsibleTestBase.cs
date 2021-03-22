using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using Responsible.Utilities;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ResponsibleTestBase
	{
		protected static readonly ITestWaitCondition<bool> ImmediateTrue =
			WaitForConditionOn("True", () => true, val => val);

		protected static readonly ITestWaitCondition<bool> Never =
			WaitForConditionOn("Never", () => false, _ => false);

		protected static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
		protected static readonly TimeSpan OneFrame = TimeSpan.FromSeconds(1.0 / 60);

		protected ILogger Logger { get; private set; }
		protected TestTimeProvider TimeProvider { get; private set; }

		protected TestInstructionExecutor Executor { get; private set; }

		protected static void Nop<T>(T unused)
		{
		}

		protected void AdvanceDefaultFrame() => this.TimeProvider.AdvanceFrame(OneFrame);

		protected static AssertionException GetAssertionException(Task task)
		{
			Assert.IsNotNull(task.Exception);
			Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
			Assert.IsInstanceOf<AssertionException>(task.Exception.InnerExceptions[0]);
			return task.Exception.InnerExceptions[0] as AssertionException;
		}

		[SetUp]
		public void BaseSetUp()
		{
			this.Logger = Substitute.For<ILogger>();
			this.TimeProvider = new TestTimeProvider();
			this.Executor = new TestInstructionExecutor(this.TimeProvider, logger: this.Logger);
		}

		[TearDown]
		public void BaseTearDown()
		{
			this.Executor.Dispose();
		}
	}
}
