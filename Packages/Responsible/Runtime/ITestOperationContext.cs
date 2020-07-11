using Responsible.Context;

namespace Responsible
{
	/// <summary>
	/// Interface for building context information for test operations.
	/// Usually used when waiting for an operation, or when a timeout or error happens.
	/// </summary>
	public interface ITestOperationContext
	{
		/// <summary>
		/// Adds a description of the operation to the builder. This should be relatively brief.
		/// </summary>
		//void BuildDescription(ContextStringBuilder builder);
	}
}