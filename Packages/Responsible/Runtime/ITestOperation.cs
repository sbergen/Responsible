using JetBrains.Annotations;
using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Base interface for the test operation types in Responsible:
	/// * <see cref="ITestInstruction{T}"/>
	/// * <see cref="ITestWaitCondition{T}"/>
	/// * <see cref="ITestResponder{T}"/>
	/// * <see cref="IOptionalTestResponder"/>
	///
	/// Test operations should be considered a set of instructions,
	/// which may be executed one or more times.
	/// This makes them reusable.
	/// </summary>
	/// <typeparam name="T">Result type of the operation.</typeparam>
	public interface ITestOperation<out T>
	{
		/// <summary>
		/// Creates the state object for a single run of this test operation.
		///
		/// You should normally prefer the following extension methods over
		/// creating a state instance manually:
		/// * <seealso cref="TestInstruction.ToYieldInstruction{T}"/>
		/// * <seealso cref="TestInstruction.ToTask{T}"/>
		/// </summary>
		/// <returns>A state object for this run of the test operation.</returns>
		/// <remarks>
		/// Converting an instance of <see cref="ITestOperationState{T}"/>
		/// to a string will dump full information of its current execution status.
		///
		/// Creating a state for a <see cref="ITestWaitCondition{T}"/> and running it directly
		/// will bypass the timeout mechanism which is otherwise enforced by Responsible,
		/// and might lead to an execution that never completes.
		/// </remarks>
		[Pure]
		[NotNull]
		ITestOperationState<T> CreateState();
	}
}
