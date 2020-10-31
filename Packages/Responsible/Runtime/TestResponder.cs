using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.TestResponders;
using UniRx;

namespace Responsible
{
	public static class TestResponder
	{
		/// <summary>
		/// Expects the responder to start responding within the given timeout,
		/// and then continues executing the instruction of the responder.
		/// </summary>
		[Pure]
		public static ITestInstruction<TResult> ExpectWithinSeconds<TResult>(
			this ITestResponder<TResult> responder,
			double timeoutSeconds,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ExpectTestResponse<TResult>(
				responder,
				TimeSpan.FromSeconds(timeoutSeconds),
				new SourceContext(nameof(ExpectWithinSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test responder to an optional test responder.
		/// An alias for <see cref="Responsibly.RespondToAnyOf{t}"/> with a single argument.
		/// </summary>
		[Pure]
		public static IOptionalTestResponder Optionally<T>(this ITestResponder<T> responder) =>
			new AnyOfResponder<T>(new[] { responder });

		/// <summary>
		/// Applies selector to result of test responder
		/// </summary>
		[Pure]
		public static ITestResponder<T2> Select<T1, T2>(
			this ITestResponder<T1> first,
			Func<T1, T2> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SelectResponder<T1, T2>(
				first,
				selector,
				new SourceContext(nameof(Select), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test responder returning any value to one returning <see cref="Unit"/>.
		/// </summary>
		[Pure]
		public static ITestResponder<Unit> AsUnitResponder<T>(
			this ITestResponder<T> responder) =>
			typeof(T) == typeof(Unit)
				? (ITestResponder<Unit>)responder
				: new UnitTestResponder<T>(responder);
	}
}