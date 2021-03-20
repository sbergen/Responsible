namespace Responsible.NoRx
{
	/// <summary>
	/// Represents a test instruction - a synchronous or asynchronous operation producing a single result when executed.
	/// All standard test instructions in Responsible have an enforced timeout.
	///
	/// See <see cref="TestInstruction"/> for extension methods for working with test instructions,
	/// and <see cref="Responsibly"/> for methods for creating basic test instructions.
	/// </summary>
	/// <typeparam name="T">Result type of the test instruction.</typeparam>
	public interface ITestInstruction<T> : ITestOperation<T>
	{
	}
}
