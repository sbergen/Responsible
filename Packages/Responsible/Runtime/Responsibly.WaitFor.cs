using System;
using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using Responsible.Context;
using Responsible.State;
using Responsible.TestInstructions;
using Responsible.TestWaitConditions;
using UniRx;

namespace Responsible
{
	/// <summary>
	/// Main class for constructing primitive wait conditions and instructions.
	/// </summary>
	/// <remarks>
	/// Instead of using a class like <c>WaitFor</c>, we try to avoid conflicting class names by having this class.
	/// It also allows you to use either <c>Responsibly.WaitForCondition</c> or
	/// <c>using static Responsible.Responsibly</c> and simply <c>WaitForCondition</c>.
	/// </remarks>
	public static partial class Responsibly
	{
		/// <summary>
		/// Constructs a wait condition, which will call an object getter on every frame,
		/// and check a condition on the returned object.
		/// Will complete once the condition returns true,
		/// returning the last value returned by the object provider.
		/// </summary>
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
		[Pure]
		public static ITestWaitCondition<Unit> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<Unit>(
				description,
				() => Unit.Default,
				_ => condition(),
				extraContext,
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a wait condition that becomes true when the given constraint
		/// is fulfilled on the object returned by <c>getObject</c>.
		/// </summary>
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
		/// Construct a wait condition, which will start the provided coroutine when executed.
		/// Will complete when the coroutine has terminated.
		/// <seealso cref="WaitForCoroutineMethod"/>
		/// </summary>
		/// <remarks>
		///	May be used with local functions and lambdas, as the description is manually provided.
		/// </remarks>
		/// <param name="description"></param>
		/// <param name="startCoroutine"></param>
		/// <param name="memberName"></param>
		/// <param name="sourceFilePath"></param>
		/// <param name="sourceLineNumber"></param>
		/// <returns></returns>
		[Pure]
		public static ITestWaitCondition<Unit> WaitForCoroutine(
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
		/// Construct a wait condition, which will start the provided coroutine when executed.
		/// Will complete when the coroutine has terminated.
		/// <seealso cref="WaitForCoroutine"/>
		/// </summary>
		/// <remarks>
		/// If used with a lambda or local function, you will get a weird compiler-generated description.
		/// </remarks>
		[Pure]
		public static ITestWaitCondition<Unit> WaitForCoroutineMethod(
			Func<IEnumerator> coroutineMethod,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new CoroutineWaitCondition(
				coroutineMethod.Method.Name,
				coroutineMethod,
				new SourceContext(nameof(WaitForCoroutineMethod), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestInstruction<Unit> WaitForSeconds(
			double seconds,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitForInstruction(
				TimeSpan.FromSeconds(seconds),
				new SourceContext(nameof(WaitForSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Wait for the given amount of WHOLE frames.
		/// Note that zero frames means to wait until the next frame.
		/// </summary>
		[Pure]
		public static ITestInstruction<Unit> WaitForFrames(
			int frames,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitForFramesInstruction(
				frames,
				new SourceContext(nameof(WaitForSeconds), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<T> WaitForLast<T>(
			string description,
			IObservable<T> observable,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ObservableWaitCondition<T>(
				description,
				observable,
				new SourceContext(nameof(WaitForLast), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Wait for all conditions to complete, and return their results as an array.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T[]> WaitForAllOf<T>(params ITestWaitCondition<T>[] conditions)
			=> new AllOfWaitCondition<T>(conditions);
	}
}
