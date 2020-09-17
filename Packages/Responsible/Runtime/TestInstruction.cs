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
	public static class TestInstruction
	{
		/// <summary>
		/// Returns an observable which executes the instruction when subscribed to.
		/// </summary>
		[Pure]
		public static IObservable<T> ToObservable<T>(
			this ITestInstruction<T> instruction,
			TestInstructionExecutor executor,
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
			TestInstructionExecutor executor,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				instruction.CreateState(),
				new SourceContext(nameof(ToYieldInstruction), memberName, sourceFilePath, sourceLineNumber))
				.ToYieldInstruction();

		/// <summary>
		/// Runs all provided test instructions in order, or until one of them fails.
		/// </summary>
		[Pure]
		public static ITestInstruction<Unit> Sequence(
			this IEnumerable<ITestInstruction<Unit>> instructions,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			instructions.Aggregate((sequencedInstructions, nextInstruction) =>
				new SequencedTestInstruction<Unit, Unit>(
					sequencedInstructions,
					nextInstruction,
					new SourceContext(nameof(Sequence), memberName, sourceFilePath, sourceLineNumber)));

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
			=> new ContinuationTestInstruction<T1, T2>(
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
			=> new SequencedTestInstruction<T1, T2>(
				first,
				second,
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
			this ITestInstruction<T> instruction) =>
			typeof(T) == typeof(Unit)
				? (ITestInstruction<Unit>)instruction
				: new UnitTestInstruction<T>(instruction);
	}
}