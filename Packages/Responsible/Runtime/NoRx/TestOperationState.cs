using Responsible.NoRx.State;

namespace Responsible.NoRx
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestOperationState"/>.
	/// These are normally not needed, but can be useful for debugging purposes.
	/// </summary>
	public static class TestOperationState
	{
		internal static ITestOperationState<Nothing> AsNothingOperationState<T>(this ITestOperationState<T> state)
			=> new NothingOperationState<T, Nothing>(state, _ => Nothing.Default);
	}
}
