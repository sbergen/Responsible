using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;

namespace Responsible
{
	/// <summary>
	/// Provides extension methods on <see cref="IOptionalTestResponder"/>.
	/// </summary>
	public static class OptionalTestResponder
	{
		/// <summary>
		/// Executes responders in <paramref name="respondTo"/>, until a condition is met.
		/// All responders are guaranteed to either complete or not start execution at all.
		/// No responders are required to execute.
		/// A failure in any of the responders, or the wait condition,
		/// will also cause the returned wait condition to complete with a failure.
		/// </summary>
		/// <returns>
		/// A wait condition which completes with the value of <paramref name="condition"/>
		/// once it and all started responders have completed.
		/// </returns>
		/// <param name="respondTo">Optional responders to execute until <paramref name="condition"/> is met.</param>
		/// <param name="condition">Condition until which responders in <paramref name="respondTo"/> are executed.</param>
		/// <typeparam name="T">Result type of both <paramref name="condition"/>, and the returned wait condition.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
		[Pure]
		public static ITestWaitCondition<T> Until<T>(
			this IOptionalTestResponder respondTo,
			ITestWaitCondition<T> condition,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new UntilResponder<T>(
				respondTo,
				condition,
				new SourceContext(nameof(Until), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Executes responders in <paramref name="respondTo"/>, until <paramref name="untilReady"/> is ready to execute,
		/// and continues executing <paramref name="untilReady"/> afterwards.
		/// All responders are guaranteed to either complete or not start execution at all.
		/// No responders in <paramref name="respondTo"/> are required to execute.
		/// A failure in any of the responders will also cause the returned responder to complete with a failure.
		/// </summary>
		/// <returns>
		/// A test responder which completes with the value of <paramref name="untilReady"/>,
		/// once it and all started responders from <paramref name="respondTo"/> have completed.
		/// </returns>
		/// <param name="respondTo">Optional responders to execute until <paramref name="untilReady"/> is ready to execute.</param>
		/// <param name="untilReady">
		/// Responder, which is executed after it is ready to execute,
		/// and any started responders in <paramref name="respondTo"/> have finished executing.
		/// </param>
		/// <typeparam name="T">Result type of both <paramref name="untilReady"/>, and the returned responder.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
		[Pure]
		public static ITestResponder<T> UntilReadyTo<T>(
			this IOptionalTestResponder respondTo,
			ITestResponder<T> untilReady,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new UntilReadyToResponder<T>(
				respondTo,
				untilReady,
				new SourceContext(nameof(UntilReadyTo), memberName, sourceFilePath, sourceLineNumber));
	}
}
