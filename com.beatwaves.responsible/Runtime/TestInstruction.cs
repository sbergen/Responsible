using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.TestWaitConditions;
using Responsible.Utilities;

namespace Responsible
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestInstruction{T}"/>,
	/// for executing, sequencing and transforming their results.
	/// </summary>
	// ReSharper disable once PartialTypeWithSinglePart, has a Unity part
	public static partial class TestInstruction
	{
		/// <summary>
		/// Starts executing the instruction as an async task.
		/// </summary>
		/// <returns>A task which will complete with the instructions status.</returns>
		/// <param name="instruction">The instruction to execute.</param>
		/// <typeparam name="T">Return type of the test instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithExecutor{T}"/>
		public static Task<T> ToTask<T>(
			this ITestInstruction<T> instruction,
			TestInstructionExecutor executor,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				instruction.CreateState(),
				new SourceContext(nameof(ToTask), memberName, sourceFilePath, sourceLineNumber),
				cancellationToken);

		/// <summary>
		/// Runs all provided test instructions in order, or until one of them fails.
		/// </summary>
		/// <returns>
		/// A test instruction which will complete with a non-null object
		/// once all provided instructions have completed, or will fail when any of the instructions fails.
		/// </returns>
		/// <param name="instructions">Instructions to sequence.</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1}"/>
		[Pure]
		public static ITestInstruction<object> Sequence(
			this IEnumerable<ITestInstruction<object>> instructions,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			string operationName = nameof(Sequence)) =>
			instructions.Aggregate((sequencedInstructions, nextInstruction) =>
				new SequencedTestInstruction<object, object>(
					sequencedInstructions,
					nextInstruction,
					new SourceContext(
						operationName, memberName, sourceFilePath, sourceLineNumber, isCollapsible: true)));

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
		/// <remarks>
		/// If the instruction fails or is canceled before <paramref name="continuation"/> has been called,
		/// the description of the second instruction isn't included in the state output, as it is unknown.
		/// Thus it is better to prefer
		/// <see cref="ContinueWith{T1,T2}(ITestInstruction{T1},ITestInstruction{T2},string,string,int)"/>
		/// when possible, which will always also include the description of the second instruction.
		/// </remarks>
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
		/// <param name="first">Instruction to execute first.</param>
		/// <param name="second">Instruction which will be executed after <paramref name="first"/>.</param>
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
		/// Applies a selector to the result of a test instruction when the instruction completes,
		/// transforming the result type to another type.
		/// </summary>
		/// <param name="instruction">A test instruction to apply the selector to.</param>
		/// <param name="selector">A function to apply to the result of the instruction.</param>
		/// <returns>
		/// A test instruction whose result is the result of invoking
		/// <paramref name="selector"/> on the result of <paramref name="instruction"/>.
		/// </returns>
		/// <typeparam name="T1">Return type of the initial instruction.</typeparam>
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
		/// Converts a test instruction returning a value type
		/// into one returning the same value boxed into <see cref="object"/>.
		/// Can be useful for example in conjunction with <see cref="Sequence"/> together with value types.
		/// </summary>
		/// <returns>
		/// A test instruction which behaves otherwise identically to <paramref name="instruction"/>,
		/// but returns its result as a boxed object.
		/// </returns>
		/// <param name="instruction">Instruction to wrap.</param>
		/// <typeparam name="T">Return type of the instruction to convert.</typeparam>
		[Pure]
		public static ITestInstruction<object> BoxResult<T>(this ITestInstruction<T> instruction)
			where T : struct
			=> new BoxedTestInstruction<T>(instruction);

		/// <summary>
		/// Wraps a test instruction to produce a state string where the state string of the original instruction
		/// is nested and indented under the given description.
		/// Most useful for sequenced instructions.
		/// </summary>
		/// <example>
		/// <code>
		/// EnterUsername()
		///     .ContinueWith(EnterPassword())
		///     .GroupAs("Log in")
		/// </code>
		///
		/// Would have a string representation similar to
		/// <code>
		/// [ ] Log in
		///   [ ] Enter username
		///   [ ] Enter password
		/// </code>
		/// </example>
		/// <param name="instruction">Instruction to wrap.</param>
		/// <param name="description">Description to use in output</param>
		/// <typeparam name="T">Return type of the instruction to wrap.</typeparam>
		/// <returns>
		/// A test instruction which behaves otherwise identically to <paramref name="instruction"/>,
		/// but produces nested.
		/// </returns>
		[Pure]
		public static ITestInstruction<T> GroupedAs<T>(
			this ITestInstruction<T> instruction,
			string description)
			=> new GroupedAsInstruction<T>(description, instruction);


		/// <summary>
		/// Repeats the <paramref name="instruction"/> until <paramref name="condition"/> is completed. 
		/// </summary>
		/// <remarks>
		/// <paramref name="condition"/> is only checked once per frame,
		/// so make sure <paramref name="instruction"/> does not complete synchronously!
		/// </remarks>
		/// <typeparam name="TWait">Return type of the wait condition.</typeparam>
		/// <typeparam name="TInstruction">Return type of the test instruction.</typeparam>
		/// <param name="instruction">Instruction to repeat.</param>
		/// <param name="condition">Condition to repeat until.</param>
		/// <param name="maximumRepeatCount">Maximum repeat count used to prevent an infinite loop.</param>		
		/// <returns>A wait condition that completes with the result of <paramref name="condition"/>.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3}"/>
		[Pure]
		public static ITestWaitCondition<TWait> RepeatUntil<TWait, TInstruction>(
			this ITestInstruction<TInstruction> instruction,
			ITestWaitCondition<TWait> condition,
			int maximumRepeatCount,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new RepeatUntilCondition<TWait, TInstruction>(
				instruction,
				condition,
				maximumRepeatCount,
				new SourceContext(nameof(RepeatUntil), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Synchronously executes a test instruction by simulating an update loop at the given frame rate.
		/// The provided tick callback will be called fro each simulated frame, with the frame duration as an argument.
		/// </summary>
		/// <remarks>
		/// Any timeouts used when executing an instruction using this method will be based on simulated time.
		/// </remarks>
		/// <param name="instruction">Instruction to run.</param>
		/// <param name="framesPerSecond">Frame rate to simulate</param>
		/// <param name="tick">Action to run on each simulated frame. Takes the frame duration as an argument.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the instruction prematurely.</param>
		/// <typeparam name="T">Return type of the instruction to execute.</typeparam>
		/// <returns>The result of the test instruction, if it completes successfully.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3,T4}"/>
		public static T RunAsSimulatedUpdateLoop<T>(
			this ITestInstruction<T> instruction,
			double framesPerSecond,
			Action<TimeSpan> tick,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			instruction.RunAsLoop(
				tick,
				cancellationToken,
				new SimulatedUpdateLoopScheduler(framesPerSecond),
				new SourceContext(nameof(RunAsSimulatedUpdateLoop), memberName, sourceFilePath, sourceLineNumber));


		/// <summary>
		/// Synchronously executes a test instruction by repeatedly calling a callback,
		/// until the instruction has completed successfully or with an error.
		/// The provided callback will be executed before any conditions are polled,
		/// and should run any actions required for the instruction to complete.
		/// </summary>
		/// <remarks>
		/// Any timeouts used when executing an instruction using this method will be based on wall-clock time.
		/// </remarks>
		/// <param name="instruction">Instruction to run.</param>
		/// <param name="tick">Action to run for each iteration of the loop.</param>
		/// <param name="cancellationToken">Optional cancellation token to cancel the instruction prematurely.</param>
		/// <typeparam name="T">Return type of the instruction to execute.</typeparam>
		/// <returns>The result of the test instruction, if it completes successfully.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3}"/>
		public static T RunAsLoop<T>(
			this ITestInstruction<T> instruction,
			Action tick,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			instruction.RunAsLoop(
				_ => tick(),
				cancellationToken,
				new GenericRunLoopScheduler(),
				new SourceContext(nameof(RunAsLoop), memberName, sourceFilePath, sourceLineNumber));

		private static T RunAsLoop<T, TTickArgument>(
			this ITestInstruction<T> instruction,
			Action<TTickArgument> tick,
			CancellationToken cancellationToken,
			RunLoopScheduler<TTickArgument> scheduler,
			SourceContext sourceContext)
		{
			using (var executor = new TestInstructionExecutor(scheduler, scheduler.ExternalResultSource))
			{
				var task = executor.RunInstruction(instruction.CreateState(), sourceContext, cancellationToken);

				while (!task.IsCompleted)
				{
					scheduler.Run(tick);
				}

				return task.GetAwaiter().GetResult();
			}
		}
	}
}
