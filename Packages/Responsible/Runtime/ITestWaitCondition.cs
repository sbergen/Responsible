namespace Responsible
{
	/// <summary>
	/// Represents a condition to wait on, which returns a result when fulfilled.
	/// </summary>
	public interface ITestWaitCondition<out T> : ITestOperation<T>
	{
	}
}