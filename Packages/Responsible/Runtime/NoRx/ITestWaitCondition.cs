namespace Responsible.NoRx
{
	/// <summary>
	/// Represents a condition to wait on, which returns a result when fulfilled.
	///
	/// See <see cref="TestWaitCondition"/> for extension methods on wait conditions,
	/// and <see cref="Responsibly"/> for methods for building basic wait conditions.
	/// </summary>
	/// <typeparam name="T">Result type of the wait condition.</typeparam>
	public interface ITestWaitCondition<T> : ITestOperation<T>
	{
	}
}
