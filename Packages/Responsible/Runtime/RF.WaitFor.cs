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
	/// RF stands for Responsible Framework.
	/// Instead of using a class like <c>WaitFor</c>, we try to avoid conflicting class names by having this class.
	/// It also allows you to use either <c>RF.WaitForCondition</c> or
	/// <c>using static Responsible.RF</c> and simply <c>WaitFor</c>.
	/// </remarks>
	public static partial class RF
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

		[Pure]
		public static ITestWaitCondition<T> WaitForAllOf<T>(
			ITestWaitCondition<T> primary,
			params ITestWaitCondition<Unit>[] secondaries)
			=> new AllOfWaitCondition<T>(primary, secondaries);

		[Pure]
		public static ITestWaitCondition<T> WaitForAllOf<T, T2>(
			ITestWaitCondition<T> primary,
			ITestWaitCondition<T2> secondary)
			=> new AllOfWaitCondition<T>(primary, secondary.AsUnitCondition());

		[Pure]
		public static ITestWaitCondition<T> WaitForAllOf<T, T2, T3>(
			ITestWaitCondition<T> primary,
			ITestWaitCondition<T2> secondary1,
			ITestWaitCondition<T3> secondary2)
			=> new AllOfWaitCondition<T>(
				primary,
				secondary1.AsUnitCondition(),
				secondary2.AsUnitCondition());

		[Pure]
		public static ITestWaitCondition<T> WaitForAllOf<T, T2, T3, T4>(
			ITestWaitCondition<T> primary,
			ITestWaitCondition<T2> secondary1,
			ITestWaitCondition<T3> secondary2,
			ITestWaitCondition<T4> secondary3)
			=> new AllOfWaitCondition<T>(
				primary,
				secondary1.AsUnitCondition(),
				secondary2.AsUnitCondition(),
				secondary3.AsUnitCondition());
	}
}