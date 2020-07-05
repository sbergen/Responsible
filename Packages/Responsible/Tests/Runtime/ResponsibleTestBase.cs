using System;
using NSubstitute;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class ResponsibleTestBase
	{
		protected const string ExceptionMessage = "THE ERROR";

		protected static readonly ITestWaitCondition<bool> ImmediateTrue =
			WaitForCondition("True", () => true, () => true);

		protected static readonly ITestWaitCondition<Unit> Never =
			WaitForCondition("Never", () => false);

		protected static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

		private IDisposable setup;

		protected ILogger Logger { get; private set; }
		protected TestScheduler Scheduler { get; private set; }

		[SetUp]
		public void BaseSetUp()
		{
			this.Logger = Substitute.For<ILogger>();
			this.Scheduler = new TestScheduler();
			this.setup = TestInstructionExtensions.OverrideExecutor(this.Scheduler, logger: this.Logger);
		}

		[TearDown]
		public void BaseTearDown()
		{
			this.setup.Dispose();
		}
	}
}