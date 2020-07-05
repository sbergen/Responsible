using System;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.TestWaitConditions;
using UniRx;

namespace Responsible
{
	public static class WaitFor
	{
		[Pure]
		public static ITestWaitCondition<T> Condition<T>(
			string description,
			Func<bool> condition,
			Func<T> makeResult,
			Action<ContextStringBuilder> extraContext = null)
			=> new PollingWaitCondition<T>(condition, description, makeResult, extraContext);

		[Pure]
		public static ITestWaitCondition<Unit> Condition(
			string description,
			Func<bool> condition,
			Action<ContextStringBuilder> extraContext = null)
			=> new PollingWaitCondition<Unit>(condition, description, () => Unit.Default, extraContext);

		[Pure]
		public static ITestInstruction<Unit> Seconds(int seconds)
			=> new WaitForInstruction(TimeSpan.FromSeconds(seconds));

		[Pure]
		public static ITestWaitCondition<T> AllOf<T>(
			ITestWaitCondition<T> primary,
			params ITestWaitCondition<Unit>[] secondaries)
			=> new AllOfWaitCondition<T>(primary, secondaries);

		[Pure]
		public static ITestWaitCondition<T> AllOf<T, T2>(
			ITestWaitCondition<T> primary,
			ITestWaitCondition<T2> secondary)
			=> new AllOfWaitCondition<T>(primary, secondary.AsUnitCondition());

		[Pure]
		public static ITestWaitCondition<T> AllOf<T, T2, T3>(
			ITestWaitCondition<T> primary,
			ITestWaitCondition<T2> secondary1,
			ITestWaitCondition<T3> secondary2)
			=> new AllOfWaitCondition<T>(
				primary,
				secondary1.AsUnitCondition(),
				secondary2.AsUnitCondition());

		[Pure]
		public static ITestWaitCondition<T> AllOf<T, T2, T3, T4>(
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