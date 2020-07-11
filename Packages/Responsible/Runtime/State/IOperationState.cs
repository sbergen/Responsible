using System;
using Responsible.Context;

namespace Responsible.State
{
	public interface IOperationState<out T>
	{
		OperationStatus Status { get; }
		IObservable<T> Execute(RunContext runContext);

		/// <summary>
		/// Adds a detailed description of the operation to the builder, which might help debugging failures.
		/// This can include a lot of detail, as it's only used when an operation fails.
		/// </summary>
		void BuildFailureContext(StateStringBuilder builder);
	}
}