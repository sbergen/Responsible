using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestResponders
{
	internal class TestResponder<TArg, T> : ITestResponder<T>
	{
		private readonly ITestWaitCondition<TArg> waitCondition;
		private readonly Func<TArg, ITestInstruction<T>> makeInstruction;
		private readonly string description;

		public IObservable<ITestInstruction<T>> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			this.waitCondition
				.WaitForResult(runContext, waitContext)
				.Select(this.makeInstruction);

		public void BuildDescription(ContextStringBuilder builder) => builder.Add(this.description);

		public void BuildFailureContext(ContextStringBuilder builder) =>
			builder.Add(this.description, this.waitCondition);

		internal TestResponder(
			string description,
			ITestWaitCondition<TArg> waitCondition,
			Func<TArg, ITestInstruction<T>> makeInstruction)
		{
			this.description = description;
			this.waitCondition = waitCondition;
			this.makeInstruction = makeInstruction;
		}
	}
}