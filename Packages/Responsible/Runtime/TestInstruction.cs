using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using Responsible.TestInstructions;
using UniRx;
using UnityEngine;

namespace Responsible
{
	public static class TestInstruction
	{
		private static readonly IScheduler DefaultScheduler = Scheduler.MainThread;
		private static readonly IObservable<Unit> DefaultPoll = Observable.EveryUpdate().AsUnitObservable();
		private static readonly ILogger DefaultLogger = Debug.unityLogger;

		private static TestInstructionExecutor executor =
			new TestInstructionExecutor(DefaultScheduler, DefaultPoll, DefaultLogger);

		/// <summary>
		/// Override the executor parameters
		/// Mostly for testing the framework itself, but might be useful elsewhere also?
		/// </summary>
		/// <remarks>
		/// Does not support being called multiple times (not intended for common use).
		/// </remarks>
		public static IDisposable OverrideExecutor(
			IScheduler scheduler = null,
			IObservable<Unit> poll = null,
			ILogger logger = null)
		{
			executor.Dispose();
			executor = new TestInstructionExecutor(
				scheduler ?? DefaultScheduler,
				poll ?? DefaultPoll,
				logger ?? DefaultLogger);
			return Disposable.Create(() => OverrideExecutor(DefaultScheduler, DefaultPoll, DefaultLogger));
		}

		/// <summary>
		/// Returns an observable which executes the instruction when subscribed to.
		/// </summary>
		[Pure]
		public static IObservable<T> ToObservable<T>(
			this ITestInstruction<T> instruction,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				instruction.CreateState(),
				new SourceContext(nameof(ToObservable), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Starts executing an instruction, and returns a yield instruction which can be waited upon.
		/// </summary>
		[Pure]
		public static ObservableYieldInstruction<T> ToYieldInstruction<T>(
			this ITestInstruction<T> instruction,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				instruction.CreateState(),
				new SourceContext(nameof(ToYieldInstruction), memberName, sourceFilePath, sourceLineNumber))
				.ToYieldInstruction();

		/// <summary>
		/// Dumping the state can be useful for debugging, so allow doing this also.
		/// </summary>
		[Pure]
		public static IObservable<T> ToObservable<T>(
			this IOperationState<T> state,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				state,
				new SourceContext(nameof(ToObservable), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Runs all provided test instructions in order, or until one of them fails.
		/// </summary>
		[Pure]
		public static ITestInstruction<Unit> Sequence(this IEnumerable<ITestInstruction<Unit>> instructions) =>
			instructions.Aggregate((acc, i) => acc.ContinueWith(_ => i));

		/// <summary>
		/// Constructs a test instruction, which will construct another instruction using <c>continuation</c>
		/// once the first instruction has completed, and then continue executing the constructed instruction.
		/// Returns the result of the second instruction.
		/// </summary>
		[Pure]
		public static ITestInstruction<T2> ContinueWith<T1, T2>(
			this ITestInstruction<T1> first,
			Func<T1, ITestInstruction<T2>> continuation,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new AggregateTestInstruction<T1, T2>(
				first,
				continuation,
				new SourceContext(nameof(ContinueWith), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Sequences to test instructions to be executed in order. Returns the result of the second instruction.
		/// </summary>
		[Pure]
		public static ITestInstruction<T2> ContinueWith<T1, T2>(
			this ITestInstruction<T1> first,
			ITestInstruction<T2> second,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new AggregateTestInstruction<T1, T2>(
				first,
				_ => second,
				new SourceContext(nameof(ContinueWith), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Applies selector to result of test instruction
		/// </summary>
		[Pure]
		public static ITestInstruction<T2> Select<T1, T2>(
			this ITestInstruction<T1> first,
			Func<T1, T2> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SelectTestInstruction<T1, T2>(
				first,
				selector,
				new SourceContext(nameof(Select), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test instruction returning any value to one returning <see cref="Unit"/>.
		/// Can be useful for example for using <see cref="Sequence"/>.
		/// </summary>
		[Pure]
		public static ITestInstruction<Unit> AsUnitInstruction<T>(
			this ITestInstruction<T> instruction,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			typeof(T) == typeof(Unit)
				? (ITestInstruction<Unit>)instruction
				: new UnitTestInstruction<T>(
					instruction,
					new SourceContext(nameof(AsUnitInstruction), memberName, sourceFilePath, sourceLineNumber));
	}
}