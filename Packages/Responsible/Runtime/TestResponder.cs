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
		/// <remarks>
		/// The timeout for the complete operation will be the sum of the given
		/// timeout and the timeout of the instruction part of the responder.
		/// </remarks>
		/// <returns>
		/// A test instruction which completes with the result of the responder,
		/// of a failure, if either the timeout is met or the responder otherwise fails.
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
		/// Applies a selector to the result of a test responder when the responder completes.
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
		/// Converts a test responder returning any value to one returning <see cref="Unit"/>.
		/// Can be useful for example in conjunction with <see cref="Responsibly.RespondToAllOf{T}"/>
		/// or <see cref="Responsibly.RespondToAnyOf{T}"/>.
		/// </summary>
		/// <returns>
		/// A test responder which behaves otherwise identically to <paramref name="responder"/>,
		/// but discards its result, and returns <see cref="Unit.Default"/> instead.
		/// </returns>
		/// <param name="responder">Test responder to wrap.</param>
		/// <remarks>
		/// When called with an responder already returning Unit, will return the responder itself.
		/// </remarks>
		/// <typeparam name="T">Return type of the responder to convert.</typeparam>
		[Pure]
		public static ITestResponder<Unit> AsUnitResponder<T>(
			this ITestResponder<T> responder) =>
			typeof(T) == typeof(Unit)
				? (ITestResponder<Unit>)responder
				: new UnitTestResponder<T>(responder);
	}
}
