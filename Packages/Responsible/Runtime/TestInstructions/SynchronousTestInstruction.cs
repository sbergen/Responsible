using System;
using NUnit.Framework;
using Responsible.Context;
using UniRx;

namespace Responsible.TestInstructions
{
	internal class SynchronousTestInstruction<T> : ITestInstruction<T>
	{
		private readonly Func<T> action;
		private readonly SourceContext sourceContext;

		public SynchronousTestInstruction(Func<T> action, SourceContext sourceContext)
		{
			this.action = action;
			this.sourceContext = sourceContext;
		}

		public IObservable<T> Run(RunContext runContext) => Observable.Create<T>(observer =>
		{
			try
			{
				observer.OnNext(this.action());
				observer.OnCompleted();
			}
			catch (Exception e)
			{
				var message = string.Join(
					TestInstructionExecutor.UnityEmptyLine,
					"Synchronous test action failed:",
					e,
					TestInstructionExecutor.InstructionStack(runContext.SourceContext(this.sourceContext)));
				observer.OnError(new AssertionException(message));
			}

			return Disposable.Empty;
		});
	}
}