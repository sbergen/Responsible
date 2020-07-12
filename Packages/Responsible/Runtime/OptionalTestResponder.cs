using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;

namespace Responsible
{
	public static class OptionalTestResponder
	{
		/// <summary>
		/// Executes responders until a condition is met.
		/// All responders are guaranteed to either not execute or complete.
		/// No responders are required to execute.
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T> Until<T>(
			this IOptionalTestResponder responder,
			ITestWaitCondition<T> condition,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new OptionalResponderWait<T>(
				responder,
				condition.CreateState(),
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Executes responders until another responder is ready to execute.
		/// All responders are guaranteed to either not execute or complete.
		/// No responders are required to execute.
		/// </summary>
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
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));
	}
}