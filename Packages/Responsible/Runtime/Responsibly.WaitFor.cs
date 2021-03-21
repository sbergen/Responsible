using System;
using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using Responsible.Context;
using Responsible.State;
using Responsible.TestInstructions;
using Responsible.TestWaitConditions;

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
		/// <typeparam name="T">Type of the object to wait on, and result of the returned wait condition.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescriptionAndContext{T1, T2}"/>
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

		/// <summary>
		/// Constructs a wait condition, which will poll a condition,
		/// and complete once the condition returns true.
		/// </summary>
		/// <returns>
		/// A wait condition which completes with <see cref="Unit.Default"/>
		/// once <paramref name="condition"/> returns true.
		/// </returns>
		/// <param name="condition">Condition to wait for, will be polled on every frame.</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescriptionAndContext{T}"/>
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
		/// Constructs a wait condition, which will call <paramref name="getObject"/> on every frame,
		/// and check <paramref name="constraint"/> on the returned object.
		/// Will complete once the constraint is fulfilled,
		/// returning the last value returned by <paramref name="getObject"/>.
		/// When constructing the state description, will add the constraint state to the description.
		/// </summary>
		/// /// <returns>
		/// A wait condition, which completes with the value last returned from <paramref name="getObject"/>,
		/// when <paramref name="constraint"/> is met for it.
		/// </returns>
		/// <param name="objectDescription">
		/// Description of the object to be tested with <paramref name="constraint"/>,
		/// to be included in the operation state description.
		/// </param>
		/// <param name="getObject">Function that returns the object to test <paramref name="constraint"/> on.</param>
		/// <param name="constraint">Constraint to check with the return value of <paramref name="getObject"/>.</param>
		/// <typeparam name="T">Type of the object to wait on, and result of the returned wait condition.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2, T3}"/>
		[Pure]
		public static ITestWaitCondition<T> WaitForConstraint<T>(
			string objectDescription,
			Func<T> getObject,
			IResolveConstraint constraint,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ConstraintWaitCondition<T>(
				objectDescription,
				getObject,
				constraint,
				new SourceContext(nameof(WaitForConstraint), memberName, sourceFilePath, sourceLineNumber));


		/// <summary>
		/// Constructs a wait condition, which will start the provided coroutine when executed.
		/// Will complete when the coroutine has terminated.
		/// See also <seealso cref="WaitForCoroutineMethod"/>.
		/// </summary>
		/// <returns>Wait condition, which completes once the coroutine has terminated.</returns>
		/// <remarks>
		///	May be used with local functions and lambdas, as the description is manually provided.
		/// </remarks>
		/// <param name="startCoroutine">Function to start the coroutine to be waited for.</param>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithDescription{T}"/>
		[Pure]
		public static ITestWaitCondition<Nothing> WaitForCoroutine(
			string description,
			Func<IEnumerator> startCoroutine,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		=> new CoroutineWaitCondition(
			description,
			startCoroutine,
			new SourceContext(nameof(WaitForCoroutine), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a wait condition, which will start the provided coroutine when executed.
		/// Will complete when the coroutine has terminated.
		/// The description will be the coroutine method name.
		/// See also <seealso cref="WaitForCoroutine"/>.
		/// </summary>
		/// <returns>Wait condition, which completes once the coroutine has terminated.</returns>
		/// <param name="coroutineMethod">Method to start the coroutine to be waited for.</param>
		/// <remarks>
		/// If used with a lambda or local function, you will get a weird compiler-generated description.
		/// </remarks>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestWaitCondition<Nothing> WaitForCoroutineMethod(
			Func<IEnumerator> coroutineMethod,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new CoroutineWaitCondition(
				coroutineMethod.Method.Name,
				coroutineMethod,
				new SourceContext(nameof(WaitForCoroutineMethod), memberName, sourceFilePath, sourceLineNumber));

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
		/// Constructs a test instruction, which will complete with <see cref="Unit.Default"/>,
		/// after the provided amount of **whole** frames.
		/// Note that zero frames means to wait until the next frame.
		/// </summary>
		/// <param name="frames">Whole frames to wait for.</param>
		/// <returns>Test instruction which completes after <paramref name="frames"/> whole frames.</returns>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T}"/>
		[Pure]
		public static ITestInstruction<Nothing> WaitForFrames(
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
		/// <typeparam name="T">Result type of the conditions to wait on</typeparam>
		[Pure]
		public static ITestWaitCondition<T[]> WaitForAllOf<T>(params ITestWaitCondition<T>[] conditions)
			=> new AllOfWaitCondition<T>(conditions);
	}
}
