using JetBrains.Annotations;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;
using UniRx;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		/// <summary>
		/// Constructs an optional test responder, which will respond to any of the given responder.
		/// While all responders are waited for concurrently, only one will be responding at a time.
		/// </summary>
		/* TODO [Pure]
		public static IOptionalTestResponder RespondToAnyOf<T>(params ITestResponder<T>[] responders) =>
			new AnyOfResponder<T>(responders);*/

		/// <summary>
		/// Constructs a wait condition that will will complete once all given responders have executed.
		/// Returns the result from the first responder
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T[]> RespondToAllOf<T>(params ITestResponder<T>[] responders) =>
			new RespondToAllOfWaitCondition<T>(responders);
	}
}