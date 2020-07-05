using JetBrains.Annotations;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;

namespace Responsible
{
	public static class MultipleResponderExtensions
	{
		[Pure]
		public static ITestWaitCondition<T> Until<T>(
			this IOptionalTestResponder responder,
			ITestWaitCondition<T> condition)
			=> new MultipleResponderWait<T>(responder, condition);

		[Pure]
		public static ITestResponder<T> UntilAbleTo<T>(
			this IOptionalTestResponder respondTo,
			ITestResponder<T> untilReady)
			=> new UntilReadyToResponder<T>(respondTo, untilReady);
	}
}