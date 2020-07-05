using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;
using UnityEngine;

namespace Responsible
{
	public static class TestInstructionExtensions
	{
		private static readonly IScheduler DefaultScheduler = Scheduler.MainThread;
		private static readonly IObservable<Unit> DefaultPoll = Observable.EveryUpdate().AsUnitObservable();
		private static readonly ILogger DefaultLogger = UnityEngine.Debug.unityLogger;

		private static readonly TestInstructionExecutor DefaultExecutor =
			new TestInstructionExecutor(DefaultScheduler, DefaultPoll, DefaultLogger);

		private static TestInstructionExecutor executor = DefaultExecutor;

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
			executor = new TestInstructionExecutor(
				scheduler ?? DefaultScheduler,
				poll ?? DefaultPoll,
				logger ?? DefaultLogger);
			return Disposable.Create(() =>  executor = DefaultExecutor);
		}

		[Pure]
		public static IObservable<T> Execute<T>(
			this ITestInstruction<T> instruction,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> instruction.Run(new RunContext(executor, new SourceContext(sourceFilePath, sourceLineNumber)));

		[Pure]
		public static ObservableYieldInstruction<T> ToYieldInstruction<T>(
			this ITestInstruction<T> instruction,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> instruction
				.Run(new RunContext(executor, new SourceContext(sourceFilePath, sourceLineNumber)))
				.ToYieldInstruction();

		[Pure]
		public static ITestInstruction<Unit> Sequence(this IEnumerable<ITestInstruction<Unit>> instructions) =>
			instructions.Aggregate((acc, i) => acc.ContinueWith(_ => i));

		[Pure]
		public static ITestInstruction<T2> ContinueWith<T1, T2>(
			this ITestInstruction<T1> first,
			Func<T1, ITestInstruction<T2>> selector)
			=> new AggregateTestInstruction<T1, T2>(first, selector);

		[Pure]
		public static ITestInstruction<T2> ContinueWith<T1, T2>(
			this ITestInstruction<T1> first,
			ITestInstruction<T2> second)
			=> new AggregateTestInstruction<T1, T2>(first, _ => second);
	}
}