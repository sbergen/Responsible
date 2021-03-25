using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExternalResultTests : ResponsibleTestBase
	{
		private static readonly ITestInstruction<int> NeverZero =
			Never.ExpectWithinSeconds(1).Select(_ => 0);

		private IExternalResultSource externalResultSource;
		private TaskCompletionSource<int> completionSource;
		private CancellationToken cancellationToken;

		[SetUp]
		public void SetUp()
		{
			this.cancellationToken = default;
		}

		protected override IExternalResultSource ExternalResultSource()
		{
			this.externalResultSource = Substitute.For<IExternalResultSource>();
			this.completionSource = new TaskCompletionSource<int>();
			this.externalResultSource
				.GetExternalResult<int>(Arg.Do<CancellationToken>(ct =>
					this.cancellationToken = ct))
				.ReturnsForAnyArgs(this.completionSource.Task);

			return this.externalResultSource;
		}

		[Test]
		public void ExternalResult_IsReturned_WhenReturnedEarly()
		{
			var task = NeverZero.ToTask(this.Executor);
			Assert.IsFalse(task.IsCompleted);

			this.completionSource.SetResult(42);
			Assert.AreEqual(42, task.AssertSynchronousResult());
		}

		[Test]
		public void ExternalResult_CausesFailure_WhenErroredEarly()
		{
			var task = NeverZero.ToTask(this.Executor);
			Assert.IsFalse(task.IsCompleted);

			var exception = new Exception("Test Exception");
			this.completionSource.SetException(exception);
			var error = GetFailureException(task);
			Assert.AreSame(exception, error.InnerException);
		}

		[Test]
		public void ExternalResult_IsCanceled_WhenFinished()
		{
			var task = Return(42).ToTask(this.Executor);
			Assert.IsTrue(task.IsCompleted);
			Assert.IsTrue(this.cancellationToken.IsCancellationRequested);
			Assert.DoesNotThrow(() => this.completionSource.SetResult(0));
		}
	}
}
