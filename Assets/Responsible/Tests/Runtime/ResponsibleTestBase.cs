using System;
using NSubstitute;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ResponsibleTestBase
	{
		protected static readonly ITestWaitCondition<bool> ImmediateTrue =
			WaitForConditionOn("True", () => true, val => val);

		protected static readonly ITestWaitCondition<Unit> Never =
			WaitForCondition("Never", () => false);

		protected static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

		protected ILogger Logger { get; private set; }
		protected TestScheduler Scheduler { get; private set; }

		protected Exception Error { get; private set; }

		protected TestInstructionExecutor Executor { get; private set; }

		protected static void Nop<T>(T unused)
		{
		}

		protected void StoreError(Exception e) => this.Error = e;

		[SetUp]
		public void BaseSetUp()
		{
			this.Logger = Substitute.For<ILogger>();
			this.Scheduler = new TestScheduler();
			this.Error = null;
			this.Executor = new TestInstructionExecutor(this.Scheduler, logger: this.Logger);
		}

		[TearDown]
		public void BaseTearDown()
		{
			this.Executor.Dispose();
		}
	}
}