using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.NoRx.Context;
using Responsible.NoRx.TestInstructions;
using Responsible.NoRx.TestResponders;
using Responsible.NoRx.Utilities;

namespace Responsible.NoRx
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestWaitCondition{T}"/>.
	/// </summary>
	public static class TestWaitCondition
	{
		/// <summary>
		/// Constructs a test instruction,
		/// which will enforce a timeout on the provided wait condition.
		/// If the condition isn't met within the timeout, the instruction will complete with an error.
		/// </summary>
		/// <returns>
		/// A test instruction which completes with the result of the condition,
		/// or a failure if either the timeout is met, or the condition completes with a failure.
		/// </returns>
		/// <param name="condition">Wait condition to wait for.</param>
		/// <param name="timeout">Timeout for waiting for the condition, in seconds.</param>
		/// <typeparam name="T">Result type of the wait condition, and the returned instruction.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
		[Pure]
		public static ITestInstruction<T> ExpectWithinSeconds<T>(
			this ITestWaitCondition<T> condition,
			double timeout,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new ExpectConditionInstruction<T>(
				condition,
				TimeSpan.FromSeconds(timeout),
				new SourceContext(nameof(ExpectWithinSeconds), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test responder, which will wait for <paramref name="condition"/> to complete,
		/// construct a test instruction using <paramref name="selector"/>,
		/// and continue executing the returned instruction.
		/// Will complete with a failure if the wait condition, selector, or constructed instruction
		/// completes with a failure.
		/// </summary>
		/// <returns>
		/// A test responder composed of <paramref name="condition"/> and <paramref name="selector"/>.
		/// </returns>
		/// <param name="condition">Condition to wait on before calling <paramref name="selector"/>.</param>
		/// <param name="description">Description of the responder which is constructed.</param>
		/// <param name="selector">
		/// Function to construct the instruction part of the responder,
		/// from the result of <paramref name="condition"/>.
		/// </param>
		/// <typeparam name="TWait">Result type of the wait condition.</typeparam>
		/// <typeparam name="TResult">Result type of the instruction and the returned responder.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3}"/>
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
		/// Constructs a test responder, which will wait for the provided condition,
		/// and then continue executing the provided instruction.
		/// Will complete with a failure if the wait condition or instruction completes with a failure.
		/// </summary>
		/// <returns>
		/// A test responder composed of <paramref name="condition"/> and <paramref name="instruction"/>.
		/// </returns>
		/// <param name="condition">Condition to wait on before executing <paramref name="instruction"/>.</param>
		/// <param name="description">Description of the responder which is constructed.</param>
		/// <param name="instruction">Instruction to execute after <paramref name="condition"/> has been met.</param>
		/// <typeparam name="TWait">Result type of the wait condition.</typeparam>
		/// <typeparam name="TResult">Result type of the instruction and the returned responder.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3}"/>
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
		/// Constructs a test responder, which will wait for the provided condition,
		/// and then continue executing a synchronous function, returning its result.
		/// Will complete with a failure if the wait condition or function completes with a failure.
		/// </summary>
		/// <returns>
		/// A test responder composed of <paramref name="condition"/> and <paramref name="selector"/>.
		/// </returns>
		/// <param name="condition">Condition to wait on before executing <paramref name="selector"/>.</param>
		/// <param name="description">Description of the responder which is constructed.</param>
		/// <param name="selector">
		/// Synchronous function to execute with the result of <paramref name="condition"/>.
		/// The returned value will be the result of the returned responder.
		/// </param>
		/// <typeparam name="TWait">Result type of the wait condition.</typeparam>
		/// <typeparam name="TResult">Result type of the instruction and the returned responder.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3}"/>
		[Pure]
		public static ITestResponder<TResult> ThenRespondWithFunc<TWait, TResult>(
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
					new SourceContext(nameof(ThenRespondWithFunc), memberName, sourceFilePath, sourceLineNumber)),
				new SourceContext(nameof(ThenRespondWithFunc), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Constructs a test responder, which will wait for the given condition,
		/// and then continue executing a synchronous action, returning <see cref="Unit.Default"/>.
		/// Will complete with a failure if the wait condition or action completes with a failure.
		/// </summary>
		/// <returns>
		/// A test responder composed of <paramref name="condition"/> and <paramref name="action"/>.
		/// </returns>
		/// <param name="condition">Condition to wait on before executing <paramref name="action"/>.</param>
		/// <param name="description">Description of the responder which is constructed.</param>
		/// <param name="action">Synchronous action to execute with the result of <paramref name="condition"/>.</param>
		/// <typeparam name="TWait">Result type of the wait condition.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1,T2,T3}"/>
		[Pure]
		public static ITestResponder<bool> ThenRespondWithAction<TWait>(
			this ITestWaitCondition<TWait> condition,
			string description,
			Action<TWait> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TestResponder<TWait, bool>(
				description,
				condition,
				waitResult => new SynchronousTestInstruction<bool>(
					description,
					action.ReturnTrue(waitResult),
					new SourceContext(nameof(ThenRespondWithAction), memberName, sourceFilePath, sourceLineNumber)),
				new SourceContext(nameof(ThenRespondWithAction), memberName, sourceFilePath, sourceLineNumber));
	}
}
