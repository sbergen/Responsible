using System;
using System.Collections;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	/*
	internal class CoroutineTestInstruction : ITestInstruction<Unit>, ITestOperationContext
	{
		private readonly SourceContext sourceContext;
		private readonly string description;

		public readonly Func<IEnumerator> StartCoroutine;
		public readonly TimeSpan Timeout;

		public void BuildDescription(ContextStringBuilder builder) => builder.Add(this.description);
		public void BuildFailureContext(ContextStringBuilder builder) => builder.NotAvailable();

		public CoroutineTestInstruction(
			Func<IEnumerator> startCoroutine,
			string description,
			TimeSpan timeout,
			SourceContext sourceContext)
		{
			this.StartCoroutine = startCoroutine;
			this.description = description;
			this.Timeout = timeout;
			this.sourceContext = sourceContext;
		}

		public IObservable<Unit> Run(RunContext runContext)
			=> runContext.Executor.WaitFor(this, runContext.SourceContext(this.sourceContext));
	}*/
}