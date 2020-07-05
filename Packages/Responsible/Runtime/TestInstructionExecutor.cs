using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;
using UnityEngine;

namespace Responsible
{
	internal class TestInstructionExecutor
	{
		// Add spaces to lines so that the Unity console doesn't strip them
		internal const string UnityEmptyLine = "\n \n";

		internal readonly ILogger Logger;
		internal readonly IScheduler Scheduler;
		internal readonly IObservable<Unit> PollObservable;

		internal TestInstructionExecutor(IScheduler scheduler, IObservable<Unit> pollObservable, ILogger logger)
		{
			this.Scheduler = scheduler;
			this.PollObservable = pollObservable;
			this.Logger = logger;
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
			var waitContext = new WaitContext(this.Scheduler);
			var logWaits = this.LogWaitFor(opContext).Subscribe();
			return makeOperation(waitContext)
				.Do(_ => this.Logger.Log(
					LogType.Log,
					string.Join(
						"\n",
						$"Finished waiting for operation in {waitContext.ElapsedTime}",
						ContextStringBuilder.MakeDescription(opContext))))
				.Timeout(timeout, this.Scheduler)
				.Catch((TimeoutException _) =>
				{
					// The Unity test runner can swallow exceptions, so both log an error and throw an exception
					var message = MakeTimeoutMessage(opContext, waitContext, sourceContext);
					this.Logger.Log(LogType.Error, $"Test operation execution failed:\n{message}");
					return Observable.Throw<TResult>(new AssertionException(message));
				})
				.Finally(logWaits.Dispose);
		});

		[Pure]
		private IObservable<Unit> LogWaitFor(ITestOperationContext context) => Observable
			.Interval(TimeSpan.FromSeconds(1), this.Scheduler)
			.Do(_ => this.Logger.Log(
				LogType.Log,
				$"Waiting for test operation:\n{ContextStringBuilder.MakeDescription(context)}"))
			.AsUnitObservable();

		[Pure]
		private static string MakeTimeoutMessage(
			ITestOperationContext opContext,
			WaitContext waitContext,
			SourceContext sourceContext)
			=> string.Join(
				UnityEmptyLine,
				ContextStringBuilder.MakeDescription(opContext),
				$"Timed out after {waitContext.ElapsedTime}",
				$"Failure context:\n{ContextStringBuilder.MakeFailureContext(opContext)}",
				$"Completed waits: \n{ContextStringBuilder.MakeCompletedList(waitContext)}",
				InstructionStack(sourceContext));
	}
}