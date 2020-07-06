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
			this.WaitFor(
				waitContext => condition.WaitForResult(new RunContext(this, context), waitContext),
				timeout,
				condition,
				context);

		[Pure]
		internal IObservable<Unit> WaitFor(
			CoroutineTestInstruction instruction,
			SourceContext context)
			=> this.WaitFor(
				_ => Observable.FromCoroutine(instruction.StartCoroutine),
				instruction.Timeout,
				instruction,
				context);

		[Pure]
		internal static string InstructionStack(SourceContext context) => $"Test instruction stack: \n{context}";

		[Pure]
		private IObservable<TResult> WaitFor<TResult>(
			Func<WaitContext, IObservable<TResult>> makeOperation,
			TimeSpan timeout,
			ITestOperationContext opContext,
			SourceContext sourceContext) => Observable.Defer(() =>
		{
			var waitContext = new WaitContext(this.Scheduler, this.pollObservable);
			var disposables = new CompositeDisposable(
				waitContext,
				this.LogWaitFor(opContext, waitContext).Subscribe());

			return makeOperation(waitContext)
				.Do(_ => this.logger.Log(
					LogType.Log,
					string.Join(
						"\n",
						$"Finished waiting for operation in {waitContext.ElapsedTime}",
						ContextStringBuilder.MakeDescription(opContext, waitContext))))
				.Timeout(timeout, this.Scheduler)
				.Catch((Exception e) =>
				{
					// The Unity test runner can swallow exceptions, so both log an error and throw an exception
					var message = e is TimeoutException
						? MakeTimeoutMessage(opContext, waitContext, sourceContext)
						: MakeErrorMessage(opContext, waitContext, sourceContext, e);
					this.logger.Log(LogType.Error, $"Test operation execution failed:\n{message}");
					return Observable.Throw<TResult>(new AssertionException(message));
				})
				.Finally(disposables.Dispose);
		});

		[Pure]
		private IObservable<Unit> LogWaitFor(ITestOperationContext context, WaitContext waitContext) => Observable
			.Interval(TimeSpan.FromSeconds(1), this.Scheduler)
			.Do(_ => this.logger.Log(
				LogType.Log,
				$"Waiting for test operation:\n{ContextStringBuilder.MakeDescription(context, waitContext)}"))
			.AsUnitObservable();

		[Pure]
		private static string MakeTimeoutMessage(
			ITestOperationContext opContext,
			WaitContext waitContext,
			SourceContext sourceContext)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(opContext, waitContext, sourceContext, "Timed out"));

		[Pure]
		private static string MakeErrorMessage(
			ITestOperationContext opContext,
			WaitContext waitContext,
			SourceContext sourceContext,
			Exception exception)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(opContext, waitContext, sourceContext, "Failed")
					.Append($"Error: {exception}"));

		[Pure]
		private static IEnumerable<string> FailureLines(
			ITestOperationContext opContext,
			WaitContext waitContext,
			SourceContext sourceContext,
			string what)
			=> new[]
			{
				ContextStringBuilder.MakeDescription(opContext, waitContext).ToString(),
				$"{what} after {waitContext.ElapsedTime}",
				$"Failure context:\n{ContextStringBuilder.MakeFailureContext(opContext, waitContext)}",
				$"Completed waits: \n{ContextStringBuilder.MakeCompletedList(waitContext)}",
				InstructionStack(sourceContext),
			};
	}
}