using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestResponders
{
	internal class TestResponder<TArg, T> : ITestResponder<T>, ITestWaitCondition<ITestInstruction<T>>
	{
		private readonly ITestWaitCondition<TArg> waitCondition;
		private readonly Func<TArg, ITestInstruction<T>> makeInstruction;
		private readonly string description;
		public ITestWaitCondition<ITestInstruction<T>> InstructionWaitCondition => this;

		public IObservable<ITestInstruction<T>> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			this.waitCondition
				.WaitForResult(runContext, waitContext)
				.Select(this.makeInstruction)
				.Do(instruction => waitContext.AddRelation(this, instruction))
				.DoOnSubscribe(() => waitContext.MarkAsStarted(this));

		public void BuildDescription(ContextStringBuilder builder) => builder.Add(this.description);

		public void BuildFailureContext(ContextStringBuilder builder)
		{
			builder.AddResponderStatus(
				this,
				this.description,
				b =>
				{
					b.Add("WAIT FOR", this.waitCondition);
					b.AddOptional(
						"AND THEN RESPOND WITH",
						b.WaitContext.RelatedContexts(this));
				});
		}

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