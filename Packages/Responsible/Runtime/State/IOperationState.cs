using System;
using Responsible.Context;

namespace Responsible.State
{
	public interface IOperationState
	{
		OperationStatus Status { get; }

		/// <summary>
		/// Adds a detailed description of the operation to the builder, which might help debugging failures.
		/// This can include a lot of detail, as it's only used when an operation fails.
		/// </summary>
		void BuildDescription(StateStringBuilder builder);
	}

	public interface IOperationState<out T> : IOperationState
	{
		IObservable<T> Execute(RunContext runContext);
	}
}