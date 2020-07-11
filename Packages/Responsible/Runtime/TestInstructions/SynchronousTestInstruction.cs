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

		private string Description => $"DO<{typeof(T).Name}>";

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
			builder.AddWithNested(this.Description, this.sourceContext.ToString());

		public void BuildFailureContext(ContextStringBuilder builder)
		{
			void ExtraContext(ContextStringBuilder b)
			{
				var e = builder.RunContext.ErrorIfFailed(this);
				if (e != null)
				{
					builder.Add($"FAILED WITH: {e.GetType().Name}");
				}
				builder.Add(this.sourceContext.ToString());
			}

			builder.AddWithNested($"<!!!> {this.Description}", ExtraContext);
		}
	}
}