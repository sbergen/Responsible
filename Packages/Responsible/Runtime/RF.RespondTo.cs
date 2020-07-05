using JetBrains.Annotations;
using Responsible.TestResponders;

namespace Responsible
{
	// See RF.WaitFor.cs for documentation
	public static partial class RF
	{
		/// <summary>
		/// Constructs an optional test responder, which will respond to any of the given responder.
		/// While all responders are waited for concurrently, only one will be responding at a time.
		/// </summary>
		[Pure]
		public static IOptionalTestResponder RespondToAnyOf<T>(params ITestResponder<T>[] responders) =>
			new AnyOfResponder<T>(responders);
	}
}