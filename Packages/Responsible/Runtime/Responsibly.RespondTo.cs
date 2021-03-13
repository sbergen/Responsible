using JetBrains.Annotations;
using Responsible.TestResponders;
using Responsible.TestWaitConditions;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		/// <summary>
		/// Constructs an optional test responder, which will respond to any of the provided responders.
		/// All responders are waited for concurrently, but only one will be responding at a time.
		/// Will complete with an error, if any of the responders completes with an error.
		///
		/// See <see cref="TestResponder.Select{T1, T2}"/> and <see cref="TestResponder.AsUnitResponder{T}"/>
		/// for methods which can be used to make the responder types match.
		/// </summary>
		/// <returns>
		/// Optional responder which executes the responders in <paramref name="responders"/> optionally, one at a time.
		/// </returns>
		/// <param name="responders">Responders to respond to.</param>
		/// <typeparam name="T">Result type of the responders.</typeparam>
		/// <remarks>
		/// The results of the responders are discarded, as returning them optionally is not trivial to implement.
		/// Returning the values of the completed responders is something that might be implemented
		/// in later versions of Responsible.
		/// </remarks>
		[Pure]
		public static IOptionalTestResponder RespondToAnyOf<T>(params ITestResponder<T>[] responders) =>
			new AnyOfResponder<T>(responders);

		/// <summary>
		/// Constructs a wait condition that will will complete once all given responders have executed.
		/// All responders are waited for concurrently, but only one will be responding at a time.
		/// Will complete with an error, if any of the responders completes with an error.
		///
		/// See <see cref="TestResponder.Select{T1, T2}"/> and <see cref="TestResponder.AsUnitResponder{T}"/>
		/// for methods which can be used to make the responder types match.
		/// </summary>
		/// <returns>
		/// A wait condition, which completes with the results of <paramref name="responders"/>
		/// once all responders have completed executing.
		/// </returns>
		/// <param name="responders">Responders to respond to.</param>
		/// <typeparam name="T">
		/// Result type of the responders.
		/// The result of the returned wait condition is an array of this type.
		/// </typeparam>
		[Pure]
		public static ITestWaitCondition<T[]> RespondToAllOf<T>(params ITestResponder<T>[] responders) =>
			new RespondToAllOfWaitCondition<T>(responders);
	}
}
