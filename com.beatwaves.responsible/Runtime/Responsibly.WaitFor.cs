using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using Responsible.TestInstructions;
using Responsible.TestWaitConditions;
using Responsible.Utilities;

namespace Responsible
{
	/// <summary>
	/// Main class for constructing primitive wait conditions and instructions,
	/// and composing responders.
	/// </summary>
	/// <remarks>
	/// Instead of using a class like <c>WaitFor</c>, we try to avoid conflicting class names by having this class.
	/// It also allows you to use either <c>Responsibly.WaitForCondition</c> or
	/// <c>using static Responsible.Responsibly;</c> and simply <c>WaitForCondition</c>.
	/// </remarks>
	public static partial class Responsibly
	{
		/// <summary>
		/// Constructs a wait condition, which will call <paramref name="getObject"/> on every frame,
		/// and check <paramref name="condition"/> on the returned object.
		/// Will complete once the condition returns true,
		/// returning the last value returned by <paramref name="getObject"/>.
		/// </summary>
		/// <returns>
		/// A wait condition, which completes with the value last returned from <paramref name="getObject"/>,
		/// when <paramref name="condition"/> returns true for it.
		/// </returns>
		/// <param name="getObject">Function that returns the object to test <paramref name="condition"/> on.</param>
		/// <param name="condition">Condition to check with the return value of <paramref name="getObject"/>.</param>
		/// <param name="extraContext">
		/// Action for producing extra context into state descriptions.
		/// The last value returned by <paramref name="getObject"/> is passed as the second argument.
		/// </param>
		/// <typeparam name="T">Type of the object to wait on, and result of the returned wait condition.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescription{T1, T2, T3}"/>
		[Pure]
		public static ITestWaitCondition<T> WaitForConditionOn<T>(
			string description,
			Func<T> getObject,
			Func<T, bool> condition,
			Action<StateStringBuilder, T> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<T>(
				description,
				getObject,
				condition,
				extraContext,
				new SourceContext(nameof(WaitForConditionOn), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a wait condition, which will poll a condition,
		/// and complete once the condition returns true.
		/// </summary>
		/// <returns>
		/// A wait condition which completes with a non-null object once <paramref name="condition"/> returns true.
		/// </returns>
		/// <param name="condition">Condition to wait for, will be polled on every frame.</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescriptionAndContext{T}"/>
		[Pure]
		public static ITestWaitCondition<object> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<object>(
				description,
				() => Unit.Instance,
				_ => condition(),
				extraContext.DiscardingState<object>(),
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test instruction, which will complete with <see cref="Unit.Instance"/>
		/// after <paramref name="seconds"/> seconds have passed.
		/// </summary>
		/// <param name="seconds">Seconds to wait for.</param>
		/// <returns>Test instruction which completes after <paramref name="seconds"/> seconds.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestInstruction<object> WaitForSeconds(
			double seconds,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitForInstruction(
				TimeSpan.FromSeconds(seconds),
				new SourceContext(nameof(WaitForSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test instruction, which will complete with a non-null object
		/// after the provided amount of **whole** frames.
		/// Note that zero frames means to wait until the next frame.
		/// </summary>
		/// <param name="frames">Whole frames to wait for.</param>
		/// <returns>Test instruction which completes after <paramref name="frames"/> whole frames.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestInstruction<object> WaitForFrames(
			int frames,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitForFramesInstruction(
				frames,
				new SourceContext(nameof(WaitForSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Wait for all conditions to complete, and return their results as an array.
		/// Will complete with an error if any of the conditions completes with an error.
		/// </summary>
		/// <returns>
		/// A wait condition which completes with the values of <paramref name="conditions"/>
		/// once they have all completed.
		/// </returns>
		/// <param name="conditions">Conditions to wait on.</param>
		/// <typeparam name="T">Result type of the conditions to wait on.</typeparam>
		[Pure]
		public static ITestWaitCondition<T[]> WaitForAllOf<T>(params ITestWaitCondition<T>[] conditions)
			=> new AllOfWaitCondition<T>(conditions);

		/// <summary>
		/// Constructs a wait condition, which will start a task, and complete with its result when it completes.
		/// </summary>
		/// <param name="taskRunner">Function used to start the task to be waited for.</param>
		/// <returns>
		/// Wait condition which completes together with the task constructed with <paramref name="taskRunner"/>.
		/// </returns>
		/// <typeparam name="T">Result type of the task to wait on.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescription{T}"/>
		[Pure]
		public static ITestWaitCondition<T> WaitForTask<T>(
			string description,
			Func<CancellationToken, Task<T>> taskRunner,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TaskWaitCondition<T>(
				description,
				taskRunner,
				new SourceContext(nameof(WaitForTask), memberName, sourceFilePath, sourceLineNumber));
	}
}
