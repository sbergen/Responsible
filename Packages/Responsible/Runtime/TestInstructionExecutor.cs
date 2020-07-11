using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;
using UnityEngine;

namespace Responsible
{
	internal class TestInstructionExecutor : IDisposable
	{
		// Add spaces to lines so that the Unity console doesn't strip them
		internal const string UnityEmptyLine = "\n \n";

		private readonly IObservable<Unit> pollObservable;
		private readonly IDisposable pollSubscription;
		private readonly ILogger logger;

		internal readonly IScheduler Scheduler;

		internal TestInstructionExecutor(IScheduler scheduler, IObservable<Unit> pollObservable, ILogger logger)
		{
			this.Scheduler = scheduler;
			this.logger = logger;

			// Workaround for how EveryUpdate works in Unity.
			// When nobody is subscribed to it, there will be a one-frame delay on the next Subscribe.
			var pollSubject = new Subject<Unit>();
			this.pollObservable = pollSubject;
			this.pollSubscription = pollObservable.Subscribe(pollSubject);
		}

		public void Dispose()
		{
			this.pollSubscription.Dispose();
		}

		[Pure]
		internal IObservable<T> WaitFor<T>(
			ITestWaitCondition<T> condition,
			TimeSpan timeout,
			SourceContext context) =>
			this.WaitFor(condition.WaitForResult, timeout, condition, context);

		[Pure]
		internal IObservable<Unit> WaitFor(
			CoroutineTestInstruction instruction,
			SourceContext context)
			=> this.WaitFor(
				(r, w) => Observable.FromCoroutine(instruction.StartCoroutine),
				instruction.Timeout,
				instruction,
				context);

		[Pure]
		internal static string InstructionStack(SourceContext context) => $"Test instruction stack: \n{context}";

		[Pure]
		private IObservable<TResult> WaitFor<TResult>(
			Func<RunContext, WaitContext, IObservable<TResult>> makeOperation,
			TimeSpan timeout,
			ITestOperationContext opContext,
			SourceContext sourceContext) => Observable.Defer(() =>
		{
			var runContext = new RunContext(this, sourceContext);
			var waitContext = new WaitContext(this.Scheduler, this.pollObservable);
			var disposables = new CompositeDisposable(
				waitContext,
				this.LogWaitFor(opContext, runContext, waitContext).Subscribe());

			return makeOperation(runContext, waitContext)
				.Do(_ => this.logger.Log(
					LogType.Log,
					string.Join(
						"\n",
						$"Finished waiting for operation in {waitContext.ElapsedTime}",
						ContextStringBuilder.MakeDescription(opContext, runContext, waitContext))))
				.Timeout(timeout, this.Scheduler)
				.Catch((Exception e) =>
				{
					// The Unity test runner can swallow exceptions, so both log an error and throw an exception
					var message = e is TimeoutException
						? MakeTimeoutMessage(opContext, runContext, waitContext, sourceContext)
						: MakeErrorMessage(opContext, runContext, waitContext, sourceContext, e);
					this.logger.Log(LogType.Error, $"Test operation execution failed:\n{message}");
					return Observable.Throw<TResult>(new AssertionException(message));
				})
				.Finally(disposables.Dispose);
		});

		[Pure]
		private IObservable<Unit> LogWaitFor(
			ITestOperationContext context,
			RunContext runContext,
			WaitContext waitContext) => Observable
			.Interval(TimeSpan.FromSeconds(1), this.Scheduler)
			.Do(_ => this.logger.Log(
				LogType.Log,
				$"Waiting for test operation:\n{ContextStringBuilder.MakeDescription(context, runContext, waitContext)}"))
			.AsUnitObservable();

		[Pure]
		private static string MakeTimeoutMessage(
			ITestOperationContext opContext,
			RunContext runContext,
			WaitContext waitContext,
			SourceContext sourceContext)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(opContext, runContext, waitContext, sourceContext, "Timed out"));

		[Pure]
		private static string MakeErrorMessage(
			ITestOperationContext opContext,
			RunContext runContext,
			WaitContext waitContext,
			SourceContext sourceContext,
			Exception exception)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(opContext, runContext, waitContext, sourceContext, "Failed")
					.Append($"Error: {exception}"));

		[Pure]
		private static IEnumerable<string> FailureLines(
			ITestOperationContext opContext,
			RunContext runContext,
			WaitContext waitContext,
			SourceContext sourceContext,
			string what)
			=> new[]
			{
				ContextStringBuilder.MakeDescription(opContext, runContext, waitContext).ToString(),
				$"{what} after {waitContext.ElapsedTime}",
				$"Failure context:\n{ContextStringBuilder.MakeFailureContext(opContext, runContext, waitContext)}",
				InstructionStack(sourceContext),
			};
	}
}