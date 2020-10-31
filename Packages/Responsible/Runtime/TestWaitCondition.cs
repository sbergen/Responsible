using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;
using Responsible.Utilities;
using UniRx;

namespace Responsible
{
	public static class TestWaitCondition
	{
		/// <summary>
		/// Constructs wait condition, which will first wait for the first condition to be fulfilled,
		/// and only then continue waiting on the second.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<TSecond> AndThen<TFirst, TSecond>(
			this ITestWaitCondition<TFirst> first,
			ITestWaitCondition<TSecond> second,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SequencedWaitCondition<TFirst, TSecond>(
				first,
				second,
				new SourceContext(nameof(AndThen), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs wait condition, which will first wait for the first condition to be fulfilled,
		/// then construct a second condition from <c>continuation</c>, and continue waiting on it.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<TSecond> AndThen<TFirst, TSecond>(
			this ITestWaitCondition<TFirst> first,
			Func<TFirst, ITestWaitCondition<TSecond>> continuation,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ContinuedWaitCondition<TFirst,TSecond>(
				first,
				continuation,
				new SourceContext(nameof(AndThen), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test responder, which will wait for the given condition,
		/// and then construct an instruction and continue executing it.
		/// </summary>
		[Pure]
		public static ITestResponder<TResult> ThenRespondWith<TWait, TResult>(
			this ITestWaitCondition<TWait> condition,
			string description,
			Func<TWait, ITestInstruction<TResult>> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TestResponder<TWait, TResult>(
				description,
				condition,
				selector,
				new SourceContext(nameof(ThenRespondWith), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test responder, which will wait for the given condition,
		/// and then continue executing the given instruction.
		/// </summary>
		[Pure]
		public static ITestResponder<TResult> ThenRespondWith<TWait, TResult>(
			this ITestWaitCondition<TWait> condition,
			string description,
			ITestInstruction<TResult> instruction,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TestResponder<TWait, TResult>(
				description,
				condition,
				_ => instruction,
				new SourceContext(nameof(ThenRespondWith), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test responder, which will wait for the given condition,
		/// and then continue executing a synchronous action.
		/// </summary>
		[Pure]
		public static ITestResponder<TResult> ThenRespondWith<TWait, TResult>(
			this ITestWaitCondition<TWait> condition,
			string description,
			Func<TWait, TResult> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TestResponder<TWait, TResult>(
				description,
				condition,
				waitResult => new SynchronousTestInstruction<TResult>(
					description,
					() => selector(waitResult),
					new SourceContext(nameof(ThenRespondWith), memberName, sourceFilePath, sourceLineNumber)),
				new SourceContext(nameof(ThenRespondWith), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test responder, which will wait for the given condition,
		/// and then continue executing a synchronous action.
		/// </summary>
		[Pure]
		public static ITestResponder<Unit> ThenRespondWith<TWait>(
			this ITestWaitCondition<TWait> condition,
			string description,
			Action<TWait> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TestResponder<TWait, Unit>(
				description,
				condition,
				waitResult => new SynchronousTestInstruction<Unit>(
					description,
					action.AsUnitFunc(waitResult),
					new SourceContext(nameof(ThenRespondWith), memberName, sourceFilePath, sourceLineNumber)),
				new SourceContext(nameof(ThenRespondWith), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a wait condition to an instruction, by enforcing a timeout on it.
		/// </summary>
		[Pure]
		public static ITestInstruction<T> ExpectWithinSeconds<T>(
			this ITestWaitCondition<T> condition,
			double timeout,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new WaitInstruction<T>(
				condition,
				TimeSpan.FromSeconds(timeout),
				new SourceContext(nameof(ExpectWithinSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Applies selector to result of wait condition
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T2> Select<T1, T2>(
			this ITestWaitCondition<T1> first,
			Func<T1, T2> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SelectWaitCondition<T1, T2>(
				first,
				selector,
				new SourceContext(nameof(Select), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a wait condition returning any return type to one returning <see cref="Unit"/>.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<Unit> AsUnitCondition<T>(
			this ITestWaitCondition<T> condition) =>
			typeof(T) == typeof(Unit)
				? (ITestWaitCondition<Unit>)condition
				: new UnitWaitCondition<T>(condition);
	}
}