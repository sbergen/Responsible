using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;

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
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ExpectTestResponse<TResult>(
				responder,
				TimeSpan.FromSeconds(timeoutSeconds),
				new SourceContext(sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test responder to an optional test responder.
		/// An alias for <see cref="RF.RespondToAnyOf{t}"/> with a single argument.
		/// </summary>
		[Pure]
		public static IOptionalTestResponder Optionally<T>(this ITestResponder<T> responder) =>
			RF.RespondToAnyOf(responder);
	}
}