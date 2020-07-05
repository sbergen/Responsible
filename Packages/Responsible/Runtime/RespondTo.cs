using Responsible.TestResponders;

namespace Responsible
{
	public static class RespondTo
	{
		public static IOptionalTestResponder AnyOf<T>(params ITestResponder<T>[] responders)
			=> new AnyOfResponder<T>(responders);

		public static IOptionalTestResponder Optionally<T>(this ITestResponder<T> responder) => AnyOf(responder);
	}
}