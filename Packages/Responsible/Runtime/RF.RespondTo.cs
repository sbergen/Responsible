using JetBrains.Annotations;
using Responsible.TestResponders;

namespace Responsible
{
	// See RF.WaitFor.cs for documentation
	public static partial class RF
	{
		[Pure]
		public static IOptionalTestResponder RespondToAnyOf<T>(params ITestResponder<T>[] responders) =>
			new AnyOfResponder<T>(responders);
	}
}