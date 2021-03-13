using System;
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
	public interface ITestOperationState<out T> : ITestOperationState
	{
		/// <summary>
		/// Starts execution of the the operation this state was created from.
		///
		/// Intended for internal use only. See
		/// * <see cref="TestInstruction.ToYieldInstruction{T}"/>,
		/// * <see cref="TestOperationState.ToYieldInstruction{T}"/>
		/// * <see cref="TestInstruction.ToObservable{T}"/>, and
		/// * <see cref="TestOperationState.ToObservable{T}"/>
		///
		/// for public ways of executing operations.
		/// </summary>
		/// <param name="runContext">The test operation run context this run is part of.</param>
		/// <returns>
		/// An observable, which will complete with the result of the operation, or an error on failure.
		/// </returns>
		IObservable<T> Execute(RunContext runContext);
	}
}
