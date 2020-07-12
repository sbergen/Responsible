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
		public static ITestWaitCondition<T> WaitForCondition<T>(
			string description,
			Func<bool> condition,
			Func<T> makeResult,
			Action<StateStringBuilder> extraContext = null)
			=> new PollingWaitCondition<T>(description, condition, makeResult, extraContext);

		[Pure]
		public static ITestWaitCondition<Unit> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<StateStringBuilder> extraContext = null)
			=> new PollingWaitCondition<Unit>(description, condition, () => Unit.Default, extraContext);

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
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));


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
			new SourceContext(memberName, sourceFilePath, sourceLineNumber));

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
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestInstruction<Unit> WaitForSeconds(
			int seconds,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitForInstruction(
				TimeSpan.FromSeconds(seconds),
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<T> WaitForLast<T>(
			string description,
			IObservable<T> observable)
			=> new ObservableWaitCondition<T>(description, observable);

		/// <summary>
		/// Wait for all conditions to complete, and return their results as an array.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T[]> WaitForAllOf<T>(params ITestWaitCondition<T>[] conditions)
			=> new AllOfWaitCondition<T>(conditions);
	}
}