using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;
using UniRx;

namespace Responsible
{
	public static class WaitConditionExtensions
	{
		[Pure]
		public static ITestWaitCondition<TSecond> AndThen<TFirst, TSecond>(
			this ITestWaitCondition<TFirst> first,
			ITestWaitCondition<TSecond> second)
			=> new SequencedWaitCondition<TFirst, TSecond>(first, second);

		[Pure]
		public static ITestWaitCondition<TSecond> AndThen<TFirst, TSecond>(
			this ITestWaitCondition<TFirst> first,
			Func<TFirst, ITestWaitCondition<TSecond>> continuation)
			=> new SequencedWaitCondition<TFirst, TSecond>(first, continuation);

		[Pure]
		public static ITestResponder<TResult> ThenRespondWith<TWait, TResult>(
			this ITestWaitCondition<TWait> condition,
			string description,
			Func<TWait, ITestInstruction<TResult>> selector)
			=> new TestResponder<TWait, TResult>(description, condition, selector);

		[Pure]
		public static ITestInstruction<T> ExpectWithinSeconds<T>(
			this ITestWaitCondition<T> condition,
			int timeout,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitInstruction<T>(
				condition,
				TimeSpan.FromSeconds(timeout),
				new SourceContext(sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestWaitCondition<Unit> AsUnitCondition<T>(this ITestWaitCondition<T> condition)
			=> new UnitWaitCondition<T>(condition);
	}
}