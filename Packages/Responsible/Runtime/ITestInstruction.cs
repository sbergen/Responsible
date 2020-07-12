namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be executed.
	/// Test instructions can be chained, and usually have an internal timeout.
	/// </summary>
	public interface ITestInstruction<out T> : ITestOperation<T>
	{
	}
}