using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;

namespace Responsible.State
{
	/// <summary>
	/// Type-agnostic parts of the execution status of a test operation,
	/// see <seealso cref="ITestOperationState{T}"/>.
	/// </summary>
	public interface ITestOperationState
	{
		/// <summary>
		/// Current status of the test operation. Intended for internal use only.
		/// </summary>
		/// <value>The current state of this test operation run.</value>
		TestOperationStatus Status { get; }

		/// <summary>
		/// Adds a detailed description of the operation to the builder.
		/// </summary>
		/// <param name="builder">State string builder to append the description of this operation state to.</param>
		void BuildDescription(StateStringBuilder builder);
	}

	/// <summary>
	/// Represents the execution status of a test operation run created with
	/// <see cref="ITestOperation{T}.CreateState"/>.
	///
	/// Mostly intended for internal use.
	/// </summary>
	/// <remarks>
	/// All implementations of <see cref="ITestOperationState{T}"/> in Responsible
	/// have an override of <c>ToString()</c>,
	/// which will produce a full textual representation of the execution state.
	/// </remarks>
	/// <typeparam name="T">Return type of the test operation.</typeparam>
	// ReSharper disable once UnusedTypeParameter, used for type inference in our covariance hack
	public interface ITestOperationState<out T> : ITestOperationState
	{
		/// <summary>
		/// Starts execution of the the operation this state was created from.
		///
		/// Intended for internal use only. See
		/// * <see cref="TestInstruction.ToYieldInstruction{T}"/>,
		/// * <see cref="TestOperationState.ToYieldInstruction{T}"/>
		/// * <see cref="TestInstruction.ToTask{T}"/>, and
		/// * <see cref="TestOperationState.ToTask{T}"/>
		///
		/// for public ways of executing operations.
		/// </summary>
		/// <param name="runContext">The test operation run context this run is part of.</param>
		/// <param name="cancellationToken">Cancellation token for canceling the run.</param>
		/// <returns>
		/// An task, which will complete with the result of the operation, or an error on failure.
		/// </returns>
		/// <remarks>
		/// Due to lack of support for covariant generic classes, or covariant generic constraints in C#,
		/// we have to use this unsafe method. However, it is used only from an extension method,
		/// which does the correct type inference for us, so overall, things are safe.
		/// </remarks>
		Task<TResult> ExecuteUnsafe<TResult>(RunContext runContext, CancellationToken cancellationToken);
			// where T : TResult <-- not supported in C#
	}
}
