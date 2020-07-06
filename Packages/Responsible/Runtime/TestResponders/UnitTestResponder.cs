using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestResponders
{
	public class UnitTestResponder<T> : ITestResponder<Unit>
	{
		private readonly ITestResponder<T> responder;

		public IObservable<ITestInstruction<Unit>> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			this.responder
				.WaitForResult(runContext, waitContext)
				.Select(instruction => instruction.AsUnitInstruction());

		public void BuildDescription(ContextStringBuilder builder) => this.responder.BuildDescription(builder);
		public void BuildFailureContext(ContextStringBuilder builder) => this.responder.BuildFailureContext(builder);

		public UnitTestResponder(ITestResponder<T> responder)
		{
			this.responder = responder;
		}
	}
}