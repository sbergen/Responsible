using System;
using System.Collections.Generic;
using System.Linq;
using Responsible.Context;
using UniRx;

namespace Responsible.TestResponders
{
	/* TODO internal class AnyOfResponder<T> : IOptionalTestResponder
	{
		private const string ContextDescription = "ANY OF";

		private readonly IReadOnlyList<ITestResponder<T>> responders;

		public AnyOfResponder(IReadOnlyList<ITestResponder<T>> responders)
		{
			this.responders = responders;
		}

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.Add(ContextDescription, this.responders);

		public void BuildFailureContext(ContextStringBuilder builder) =>
			builder.Add(ContextDescription, this.responders);

		public IObservable<IObservable<Unit>> Instructions(RunContext runContext, WaitContext waitContext) =>
			this.responders
				.Select(responder => responder.InstructionWaitCondition
					.WaitForResult(runContext, waitContext))
				.Merge()
				.Select(instruction => instruction
					.Run(runContext)
					.AsUnitObservable());
	}*/
}