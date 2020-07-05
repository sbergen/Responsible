using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

namespace Responsible
{
	public static class TestInstructionExtensions
	{
		private static readonly TestInstructionExecutor Executor = new TestInstructionExecutor(
			Scheduler.MainThread,
			Observable.EveryUpdate().AsUnitObservable());

		[Pure]
		public static IObservable<T> Execute<T>(
			this ITestInstruction<T> instruction,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> instruction.Run(new RunContext(Executor, new SourceContext(sourceFilePath, sourceLineNumber)));

		[Pure]
		public static ObservableYieldInstruction<T> ToYieldInstruction<T>(
			this ITestInstruction<T> instruction,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> instruction
				.Run(new RunContext(Executor, new SourceContext(sourceFilePath, sourceLineNumber)))
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