using JetBrains.Annotations;
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
			ITestWaitCondition<T> condition)
			=> new MultipleResponderWait<T>(responder, condition);

		/// <summary>
		/// Executes responders until another responder is ready to execute.
		/// All responders are guaranteed to either not execute or complete.
		/// No responders are required to execute.
		/// </summary>
		[Pure]
		public static ITestResponder<T> UntilReadyTo<T>(
			this IOptionalTestResponder respondTo,
			ITestResponder<T> untilReady)
			=> new UntilReadyToResponder<T>(respondTo, untilReady);
	}
}