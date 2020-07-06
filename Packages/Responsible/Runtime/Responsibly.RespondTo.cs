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
		[Pure]
		public static IOptionalTestResponder RespondToAnyOf<T>(params ITestResponder<T>[] responders) =>
			new AnyOfResponder<T>(responders);

		/// <summary>
		/// Constructs a wait condition that will will complete once all given responders have executed.
		/// Returns the result from the first responder
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T> RespondToAllOf<T>(
			ITestResponder<T> primary,
			params ITestResponder<Unit>[] secondaries) =>
			new RespondToAllOfWaitCondition<T>(primary, secondaries);

		/// <summary>
		/// Constructs a wait condition that will will complete once all given responders have executed.
		/// Returns the result from the first responder
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T> RespondToAllOf<T, T2>(
			ITestResponder<T> primary,
			ITestResponder<T2> secondary) =>
			new RespondToAllOfWaitCondition<T>(primary, secondary.AsUnitResponder());

		/// <summary>
		/// Constructs a wait condition that will will complete once all given responders have executed.
		/// Returns the result from the first responder
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T> RespondToAllOf<T, T2, T3>(
			ITestResponder<T> primary,
			ITestResponder<T2> secondary1,
			ITestResponder<T3> secondary2) =>
			new RespondToAllOfWaitCondition<T>(
				primary,
				secondary1.AsUnitResponder(),
				secondary2.AsUnitResponder());

		/// <summary>
		/// Constructs a wait condition that will will complete once all given responders have executed.
		/// Returns the result from the first responder
		/// </summary>
		[Pure]
		public static ITestWaitCondition<T> RespondToAllOf<T, T2, T3, T4>(
			ITestResponder<T> primary,
			ITestResponder<T2> secondary1,
			ITestResponder<T3> secondary2,
			ITestResponder<T4> secondary3) =>
			new RespondToAllOfWaitCondition<T>(
				primary,
				secondary1.AsUnitResponder(),
				secondary2.AsUnitResponder(),
				secondary3.AsUnitResponder());
	}
}