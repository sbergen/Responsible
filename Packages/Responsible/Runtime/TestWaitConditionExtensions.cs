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
		/// <summary>
		/// Constructs wait condition, which will first wait for the first condition to be fulfilled,
		/// and only then continue waiting on the second.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<TSecond> AndThen<TFirst, TSecond>(
			this ITestWaitCondition<TFirst> first,
			ITestWaitCondition<TSecond> second)
			=> new SequencedWaitCondition<TFirst, TSecond>(first, second);

		/// <summary>
		/// Constructs wait condition, which will first wait for the first condition to be fulfilled,
		/// then construct a second condition from <c>continuation</c>, and continue waiting on it.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<TSecond> AndThen<TFirst, TSecond>(
			this ITestWaitCondition<TFirst> first,
			Func<TFirst, ITestWaitCondition<TSecond>> continuation)
			=> new SequencedWaitCondition<TFirst, TSecond>(first, continuation);

		/// <summary>
		/// Constructs a test responder, which will wait for the given condition,
		/// and then continue executing the given instruction.
		/// </summary>
		[Pure]
		public static ITestResponder<TResult> ThenRespondWith<TWait, TResult>(
			this ITestWaitCondition<TWait> condition,
			string description,
			Func<TWait, ITestInstruction<TResult>> selector)
			=> new TestResponder<TWait, TResult>(description, condition, selector);

		/// <summary>
		/// Converts a wait condition to an instruction, by enforcing a timeout on it.
		/// </summary>
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

		/// <summary>
		/// Converts a wait condition returning any return type to one returning <see cref="Unit"/>.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<Unit> AsUnitCondition<T>(this ITestWaitCondition<T> condition)
			=> new UnitWaitCondition<T>(condition);
	}
}