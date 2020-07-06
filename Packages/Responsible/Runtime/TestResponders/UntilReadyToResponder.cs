using System;
using Responsible.Context;
using Responsible.TestWaitConditions;

namespace Responsible.TestResponders
{
	internal class UntilReadyToResponder<T> : ITestResponder<T>
	{
		private readonly ITestWaitCondition<ITestInstruction<T>> respondTo;
		private readonly ITestResponder<T> untilReady;

		public IObservable<ITestInstruction<T>> WaitForResult(RunContext runContext, WaitContext waitContext)
			=> this.respondTo.WaitForResult(runContext, waitContext);

		public void BuildDescription(ContextStringBuilder builder)
		{
			builder.Add("RESPONDING TO", this.respondTo);
			builder.Add("UNTIL READY TO", this.untilReady);
		}

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);

		public UntilReadyToResponder(IOptionalTestResponder respondTo, ITestResponder<T> untilReady)
		{
			this.respondTo = new OptionalResponderWait<ITestInstruction<T>>(respondTo, untilReady);
			this.untilReady = untilReady;
		}
	}
}