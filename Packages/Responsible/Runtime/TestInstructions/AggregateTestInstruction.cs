using System;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class AggregateTestInstruction<T1, T2> : ITestInstruction<T2>
	{
		private readonly ITestInstruction<T1> first;
		private readonly Func<T1, ITestInstruction<T2>> selector;

		public AggregateTestInstruction(
			ITestInstruction<T1> first,
			Func<T1, ITestInstruction<T2>> selector)
		{
			this.first = first;
			this.selector = selector;
		}

		public IObservable<T2> Run(RunContext runContext) => this.first
			.Run(runContext)
			.ContinueWith(result =>
			{
				var instruction = this.selector(result);
				runContext.AddRelation(this, instruction);
				return instruction.Run(runContext);
			});

		public void BuildDescription(ContextStringBuilder builder)
		{
			builder.Add("FIRST", this.first);
			builder.AddOptional(
				"AND THEN",
				builder.RunContext.RelatedContexts(this));
		}

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);
	}
}