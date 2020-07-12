using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Responsible.Context;
using Responsible.State;
using UniRx;
using UnityEngine;

namespace Responsible
{
	internal class TestInstructionExecutor : IDisposable
	{
		// Add spaces to lines so that the Unity console doesn't strip them
		private const string UnityEmptyLine = "\n \n";

		private readonly IDisposable pollSubscription;
		private readonly ILogger logger;
		private readonly IScheduler scheduler;
		private readonly IObservable<Unit> pollObservable;

		internal TestInstructionExecutor(IScheduler scheduler, IObservable<Unit> pollObservable, ILogger logger)
		{
			this.scheduler = scheduler;
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

		internal IObservable<T> RunInstruction<T>(
			ITestOperationState<T> rootState,
			SourceContext sourceContext)
		{
			var runContext = new RunContext(sourceContext, this.scheduler, this.pollObservable);
			return rootState
				.Execute(runContext)
				.Catch((Exception e) =>
				{
					// The Unity test runner can swallow exceptions, so both log an error and throw an exception
					var message = e is TimeoutException
						? MakeTimeoutMessage(rootState)
						: MakeErrorMessage(rootState, e);
					this.logger.Log(LogType.Error, $"Test operation execution failed:\n{message}");
					return Observable.Throw<T>(new AssertionException(message));
				});
		}

		[Pure]
		private static string MakeTimeoutMessage<T>(ITestOperationState<T> rootOperation)
			=> string.Join(UnityEmptyLine, FailureLines(rootOperation, "timed out"));

		[Pure]
		private static string MakeErrorMessage<T>(
			ITestOperationState<T> rootOperation,
			Exception exception)
			=> string.Join(
				UnityEmptyLine,
				FailureLines(rootOperation,"failed").Append($"Error: {exception}"));

		[Pure]
		private static IEnumerable<string> FailureLines<T>(
			ITestOperationState<T> rootOperation,
			string what)
			=> new[]
			{
				$"Test instruction {what}!",
				$"Failure context:\n{StateStringBuilder.MakeState(rootOperation)}",
			};
	}
}