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
	/// <summary>
	/// Contains extension methods on <see cref="ITestInstruction{T}"/>,
	/// for executing, sequencing and transforming their results.
	/// </summary>
	public static class TestInstruction
	{
		/// <summary>
		/// Constructs an observable, which executes the instruction when subscribed to.
		/// </summary>
		/// <returns>A cold observable that runs the instruction when subscribed to.</returns>
		/// <param name="instruction">The instruction to wrap into an observable.</param>
		/// <param name="executor">Executor to use for executing the instruction.</param>
		/// <typeparam name="T">Return type of the test instruction to wrap.</typeparam>
		/// /// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
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
		/// Starts executing an instruction, and returns a yield instruction which can be awaited,
		/// using <c>yield return</c> in a Unity coroutine.
		/// </summary>
		/// <returns>Yield instruction for Unity, which will complete when the instruction has completed.</returns>
		/// <param name="instruction">Instruction to execute.</param>
		/// <param name="executor">Executor to use for executing the instruction.</param>
		/// <typeparam name="T">Return type of the test instruction to start.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
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
		/// <returns>
		/// A test instruction which will complete with <see cref="Unit.Default"/>
		/// once all provided instructions have completed, or will fail when any of the instructions fails.
		/// </returns>
		/// <param name="instructions">Instructions to sequence.</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1}"/>
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
		/// Constructs a test instruction,
		/// which will first construct another instruction using <paramref name="continuation"/>
		/// from the result of the first instruction, and then continue executing the constructed instruction.
		/// The continuation will not be called, if the first instruction fails.
		/// </summary>
		/// <param name="first">Instruction to execute first.</param>
		/// <param name="continuation">
		/// Function that builds the continuation instruction from the result of the first instruction.
		/// </param>
		/// <returns>
		/// A test instruction that completes with the result from the continuation instruction,
		/// once both instructions have completed executing.
		/// </returns>
		/// <typeparam name="T1">Return type of the first instruction.</typeparam>
		/// <typeparam name="T2">Return type of the continuation instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2}"/>
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
		/// Sequences two test instructions to be executed in order,
		/// into one instruction returning the result of the second instruction.
		/// The second instruction will not execute, if the first instruction fails.
		/// </summary>
		/// <param name="first">Instruction to execute first</param>
		/// <param name="second">Instruction which will be executed after <paramref name="first"/></param>
		/// <returns>
		/// A test instruction that completes with the result from the second instruction,
		/// once both instructions have completed executing.
		/// </returns>
		/// <typeparam name="T1">Return type of the first instruction.</typeparam>
		/// <typeparam name="T2">Return type of the second instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2}"/>
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
		/// Applies a selector to the result of a test instruction when the instruction completes.
		/// </summary>
		/// <param name="instruction">A test instruction to apply the selector to.</param>
		/// <param name="selector">A function to apply to the result of the instruction.</param>
		/// <returns>
		/// A test instruction whose result is the result of invoking
		/// <paramref name="selector"/> on the result of <paramref name="instruction"/>.
		/// </returns>
		/// <typeparam name="T1">Return type of the instruction.</typeparam>
		/// <typeparam name="T2">Return type of the selector and the returned instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2}"/>
		[Pure]
		public static ITestInstruction<T2> Select<T1, T2>(
			this ITestInstruction<T1> instruction,
			Func<T1, T2> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SelectTestInstruction<T1, T2>(
				instruction,
				selector,
				new SourceContext(nameof(Select), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test instruction returning any value to one returning <see cref="Unit"/>.
		/// Can be useful for example for using <see cref="Sequence"/>.
		/// </summary>
		/// <returns>
		/// A test instruction which behaves otherwise identically to <paramref name="instruction"/>,
		/// but discards its result, and returns <see cref="Unit.Default"/> instead.
		/// </returns>
		/// <param name="instruction">Instruction to wrap.</param>
		/// <remarks>
		/// When called with an instruction already returning Unit, will return the instruction itself.
		/// </remarks>
		/// <typeparam name="T">Return type of the instruction to convert.</typeparam>
		[Pure]
		public static ITestInstruction<Unit> AsUnitInstruction<T>(
			this ITestInstruction<T> instruction) =>
			typeof(T) == typeof(Unit)
				? (ITestInstruction<Unit>)instruction
				: new UnitTestInstruction<T>(instruction);
	}
}
