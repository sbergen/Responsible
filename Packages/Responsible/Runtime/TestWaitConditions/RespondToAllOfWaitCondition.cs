using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

namespace Responsible.TestWaitConditions
{
	public class RespondToAllOfWaitCondition<T> : ITestWaitCondition<T>
	{
		private readonly ITestResponder<T> primary;
		private readonly IReadOnlyList<ITestResponder<Unit>> secondaries;

		private IEnumerable<ITestOperationContext> AllContexts =>
			this.secondaries.Cast<ITestOperationContext>().Prepend(this.primary);

		public RespondToAllOfWaitCondition(ITestResponder<T> primary, params ITestResponder<Unit>[] secondaries)
		{
			this.primary = primary;
			this.secondaries = secondaries;
		}

		public IObservable<T> WaitForResult(RunContext runContext, WaitContext waitContext) =>
			this.primary.InstructionWaitCondition
				.WaitForResult(runContext, waitContext)
				.Select(instruction => (isPrimary: true, instruction: instruction.AsUnitInstruction()))
				.Merge(this.secondaries
					.Select(secondary => secondary.InstructionWaitCondition
						.WaitForResult(runContext, waitContext)
						.Select(instruction => (isPrimary: false, instruction)))
					.Merge())
				.Select(data => data.isPrimary
					? ((UnitTestInstruction<T>)data.instruction).Instruction.Run(runContext)
					: data.instruction.Run(runContext).SelectMany(_ => Observable.Empty<T>()))
				.Concat() // Sequence execution
				.Last(); // Defer publishing value until completed

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.Add("RESPOND TO ALL OF", this.AllContexts);

		public void BuildFailureContext(ContextStringBuilder builder) =>
			this.BuildDescription(builder);
	}
}