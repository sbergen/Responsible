using System;
using NUnit.Framework;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class SynchronousTestInstruction<T> : ITestInstruction<T>
	{
		private readonly string description;
		private readonly Func<T> action;
		private readonly SourceContext sourceContext;

		public SynchronousTestInstruction(string description, Func<T> action, SourceContext sourceContext)
		{
			this.description = description;
			this.action = action;
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Run(RunContext runContext) => Observable.Create<T>(observer =>
		{
			try
			{
				observer.OnNext(this.action());
				runContext.MarkAsCompleted(this);
				observer.OnCompleted();
			}
			catch (Exception e)
			{
				runContext.MarkAsFailed(this, e);

				var message = string.Join(
					TestInstructionExecutor.UnityEmptyLine,
					"Synchronous test action failed:",
					e,
					TestInstructionExecutor.InstructionStack(runContext.SourceContext(this.sourceContext)));
				observer.OnError(new AssertionException(message));
			}

			return Disposable.Empty;
		});

		public void BuildDescription(ContextStringBuilder builder) =>
			builder.AddWithNested(this.description, this.sourceContext.ToString());

		public void BuildFailureContext(ContextStringBuilder builder)
			=> builder.AddInstructionStatus(this, this.sourceContext, this.description);
	}
}