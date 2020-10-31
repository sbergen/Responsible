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
	/// <c>using static Responsible.Responsibly</c> and simply <c>WaitFor</c>.
	/// </remarks>
	public static partial class Responsibly
	{
		[Pure]
		public static ITestWaitCondition<TResult> WaitForConditionOn<TObject, TResult>(
			string description,
			Func<TObject> getObject,
			Func<TObject, bool> condition,
			Func<TObject, TResult> makeResult,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<TObject, TResult>(
				description,
				getObject,
				condition,
				makeResult,
				extraContext,
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<T> WaitForConditionOn<T>(
			string description,
			Func<T> getObject,
			Func<T, bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<T, T>(
				description,
				getObject,
				condition,
				_ => _,
				extraContext,
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<T> WaitForCondition<T>(
			string description,
			Func<bool> condition,
			Func<T> makeResult,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<Unit, T>(
				description,
				() => Unit.Default,
				_ => condition(),
				_ => makeResult(),
				extraContext,
				new SourceContext(nameof(WaitForCondition), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<Unit> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<StateStringBuilder> extraContext = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new PollingWaitCondition<Unit, Unit>(
				description,
				() => Unit.Default,
				_ => condition(),
				_ => Unit.Default,
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

		[Pure]
		public static ITestWaitCondition<Unit> WaitForCoroutine(
			Func<IEnumerator> startCoroutine,
			[CanBeNull] string description = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		=> new CoroutineWaitCondition<Unit>(
			description,
			startCoroutine,
			() => Unit.Default,
			new SourceContext(nameof(WaitForCoroutine), memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<T> WaitForCoroutine<T>(
			Func<IEnumerator> startCoroutine,
			Func<T> makeResult,
			[CanBeNull] string description = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new CoroutineWaitCondition<T>(
				description,
				startCoroutine,
				makeResult,
				new SourceContext(nameof(WaitForCoroutine), memberName, sourceFilePath, sourceLineNumber));

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