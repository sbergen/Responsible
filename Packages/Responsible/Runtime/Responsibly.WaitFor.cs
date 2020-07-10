using System;
using JetBrains.Annotations;
using Responsible.Context;
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
			Action<ContextStringBuilder> extraContext = null)
			=> new PollingWaitCondition<T>(condition, description, makeResult, extraContext);

		[Pure]
		public static ITestWaitCondition<Unit> WaitForCondition(
			string description,
			Func<bool> condition,
			Action<ContextStringBuilder> extraContext = null)
			=> new PollingWaitCondition<Unit>(condition, description, () => Unit.Default, extraContext);

		[Pure]
		public static ITestInstruction<Unit> WaitForSeconds(int seconds)
			=> new WaitForInstruction(TimeSpan.FromSeconds(seconds));

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