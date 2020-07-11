using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be conditionally executed, if some condition is met.
	/// Usually constructed from a <see cref="ITestWaitCondition{T}"/> and <see cref="ITestInstruction{T}"/>
	/// </summary>
	public interface ITestResponder<out T> : ITestOperationContext
	{
		// Design note: This interface is not just an alias for ITestWaitCondition<ITestInstruction<T>>
		// in order to make it less easy to end up accidentally waiting for an ITestInstruction<T>.
		// This property should almost always be used only internally.
		IOperationState<IOperationState<T>> CreateState();
	}
}