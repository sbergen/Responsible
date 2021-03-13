namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be executed one more more times.
	/// All standard test instructions in Responsible have an enforced timeout.
	///
	/// See <see cref="TestInstruction"/> for extension methods for working with test instructions,
	/// and <see cref="Responsibly"/> for methods for creating basic test instructions.
	/// </summary>
	/// <typeparam name="T">Result type of the test instruction.</typeparam>
	public interface ITestInstruction<out T> : ITestOperation<T>
	{
	}
}
