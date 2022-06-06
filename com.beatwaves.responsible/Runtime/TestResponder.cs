using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.TestResponders;

namespace Responsible
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestResponder{T}"/>.
	/// </summary>
	public static class TestResponder
	{
		/// <summary>
		/// Constructs a test instruction,
		/// which will expect the provided responder to start responding within the provided timeout,
		/// and then continue executing the instruction of the responder.
		/// If the condition isn't met within the timeout, the instruction will complete with an error.
		/// </summary>
		/// <remarks>
		/// The timeout for the complete operation will be the sum of the given
		/// timeout and the timeout of the instruction part of the responder.
		/// </remarks>
		/// <returns>
		/// A test instruction which completes with the result of the responder,
		/// or a failure, if either the timeout is met or the responder otherwise fails.
		/// </returns>
		/// <param name="responder">Responder to apply the timeout to.</param>
		/// <param name="timeoutSeconds">Timeout for the responder to start responding, in seconds.</param>
		/// <typeparam name="T">Result type of the responder and the returned test instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
		[Pure]
		public static ITestInstruction<T> ExpectWithinSeconds<T>(
			this ITestResponder<T> responder,
			double timeoutSeconds,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ExpectTestResponse<T>(
				responder,
				TimeSpan.FromSeconds(timeoutSeconds),
				new SourceContext(nameof(ExpectWithinSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test responder to an optional test responder.
		/// An alias for <see cref="Responsibly.RespondToAnyOf{t}"/> with a single argument.
		/// </summary>
		/// <returns>An optional responder, which optionally responds to <paramref name="responder"/>.</returns>
		/// <param name="responder">Responder to convert to an optional responder.</param>
		/// <typeparam name="T">
		/// Result type of the responder, which is discarded by the returned optional responder.
		/// </typeparam>
		[Pure]
		public static IOptionalTestResponder Optionally<T>(this ITestResponder<T> responder) =>
			new AnyOfResponder<T>(new[] { responder });

		/// <summary>
		/// Converts a test responder to an optional test responder which repeatedly executes the responder,
		/// zero or more times.
		/// </summary>
		/// <remarks>
		/// Unless this is the intended behaviour,
		/// make sure that the responder is not synchronously ready to execute after finishing,
		/// as otherwise it will execute again in the same frame.
		/// The <paramref name="maximumRepeatCount"/> parameter is non-optional,
		/// in order to bail out of these situations faster, as timeouts do not apply within a single frame
		/// (the timeout mechanism might be improved later, but is currently not trivial to do).
		/// </remarks>
		/// <returns>An optional responder, which repeatedly responds to <paramref name="respondTo"/>.</returns>
		/// <param name="respondTo">Responder to convert to a repeated responder.</param>
		/// <param name="maximumRepeatCount">
		/// Maximum number of times this responder is allowed to be executed.
		/// If this count is exceeded, the operation will fail.
		/// </param>
		/// <typeparam name="T">
		/// Result type of the responder, which is discarded by the returned optional responder.
		/// </typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2}"/>
		[Pure]
		public static IOptionalTestResponder Repeatedly<T>(
			this ITestResponder<T> respondTo,
			int maximumRepeatCount,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new RepeatedlyResponder<T>(
				respondTo,
				maximumRepeatCount,
				new SourceContext(nameof(Repeatedly), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Applies a selector to the result of a test responder when the responder completes,
		/// transforming the result type to another type.
		/// </summary>
		/// <param name="responder">A test responder to apply the selector to.</param>
		/// <param name="selector">A function to apply to the result of the responder.</param>
		/// <returns>
		/// A test responder whose result is the result of invoking
		/// <paramref name="selector"/> on the result of <paramref name="responder"/>.
		/// </returns>
		/// <typeparam name="T1">Return type of the initial responder.</typeparam>
		/// <typeparam name="T2">Return type of the selector and the returned responder.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2}"/>
		[Pure]
		public static ITestResponder<T2> Select<T1, T2>(
			this ITestResponder<T1> responder,
			Func<T1, T2> selector,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SelectResponder<T1, T2>(
				responder,
				selector,
				new SourceContext(nameof(Select), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Converts a test responder returning a value type
		/// into one returning the same value boxed into <see cref="object"/>.
		/// Can be useful for example in conjunction with <see cref="Responsibly.RespondToAllOf{T}"/>
		/// </summary>
		/// <returns>
		/// A test responder which behaves otherwise identically to <paramref name="responder"/>,
		/// but returns its result as a boxed object.
		/// </returns>
		/// <param name="responder">Test responder to wrap.</param>
		/// <typeparam name="T">Return type of the responder to convert.</typeparam>
		[Pure]
		public static ITestResponder<object> BoxResult<T>(this ITestResponder<T> responder)
			where T : struct
			=> new BoxedTestResponder<T>(responder);
	}
}
