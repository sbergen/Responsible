using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Represents a test instruction which can be conditionally executed, if some condition is met.
	/// Usually constructed from a <see cref="ITestWaitCondition{T}"/> and <see cref="ITestInstruction{T}"/>
	///
	/// See <see cref="TestResponder"/> for extension methods on test responders,
	/// <see cref="TestWaitCondition"/> for methods for composing wait conditions and instructions into responders, and
	/// <see cref="Responsibly"/> for methods used to combine multiple test responders.
	/// </summary>
	/// <typeparam name="T">Result type of the test responder.</typeparam>
	public interface ITestResponder<out T> : ITestOperation<ITestOperationState<T>>
	{
	}
}
