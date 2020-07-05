namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be conditionally executed, if some condition is met.
	/// Usually constructed from a <see cref="ITestWaitCondition{T}"/> and <see cref="ITestInstruction{T}"/>
	/// </summary>
	/// <remarks>
	/// This is really just an alias to make code look cleaner and add semantics.
	/// </remarks>
	public interface ITestResponder<out T> : ITestWaitCondition<ITestInstruction<T>>
	{
	}
}