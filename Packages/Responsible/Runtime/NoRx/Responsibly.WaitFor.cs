using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;
using Responsible.NoRx.TestInstructions;
using Responsible.NoRx.TestWaitConditions;

namespace Responsible.NoRx
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
		[Pure]
		public static ITestWaitCondition<T> WaitForConditionOn<T>(
			string description,
			Func<T> getObject,
			Func<T, bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<T>(
				description,
				getObject,
				condition,
				extraContext,
				new SourceContext(nameof(WaitForConditionOn), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<Nothing> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<Nothing>(
				description,
				() => Nothing.Default,
				_ => condition(),
				extraContext,
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test instruction, which will complete with <see cref="Nothing.Default"/>
		/// after <paramref name="seconds"/> seconds have passed.
		/// </summary>
		/// <param name="seconds">Seconds to wait for.</param>
		/// <returns>Test instruction which completes after <paramref name="seconds"/> seconds.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestInstruction<Nothing> WaitForSeconds(
			double seconds,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitForInstruction(
				TimeSpan.FromSeconds(seconds),
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
		/// <typeparam name="T">Result type of the conditions to wait on</typeparam>
		[Pure]
		public static ITestWaitCondition<T[]> WaitForAllOf<T>(params ITestWaitCondition<T>[] conditions)
			=> new AllOfWaitCondition<T>(conditions);
	}
}
