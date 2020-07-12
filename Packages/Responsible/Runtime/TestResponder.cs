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
			int timeoutSeconds,
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
			Responsibly.RespondToAnyOf(responder);

		/// <summary>
		/// Converts a test responder returning any value to one returning <see cref="Unit"/>.
		/// </summary>
		[Pure]
		public static ITestResponder<Unit> AsUnitResponder<T>(
			this ITestResponder<T> responder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			typeof(T) == typeof(Unit)
				? (ITestResponder<Unit>)responder
				: new UnitTestResponder<T>(
					responder,
					new SourceContext(nameof(AsUnitResponder), memberName, sourceFilePath, sourceLineNumber));
	}
}