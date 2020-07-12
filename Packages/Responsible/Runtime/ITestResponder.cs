using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be conditionally executed, if some condition is met.
	/// Usually constructed from a <see cref="ITestWaitCondition{T}"/> and <see cref="ITestInstruction{T}"/>
	/// </summary>
	public interface ITestResponder<out T> : ITestOperation<ITestOperationState<T>>
	{
	}
}