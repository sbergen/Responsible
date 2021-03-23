using System.Threading;
using System.Threading.Tasks;

namespace Responsible
{
	/// <summary>
	/// Interface for making test operations succeed or fail early.
	/// </summary>
	/// <remarks>
	/// Mostly intended for Unity, where the default mode is to fail tests on logged errors or exceptions.
	/// </remarks>
	public interface IExternalResultSource
	{
		/// <summary>
		/// This method will be called when starting to execute a test instruction
		/// with <see cref="TestInstructionExecutor"/>.
		/// If it completes with an error or result, the test instruction will be canceled,
		/// and the root operation will complete with the returned result or failure.
		/// </summary>
		/// <param name="cancellationToken">
		/// Cancellation token which should cancel the external result operation.
		/// </param>
		/// <typeparam name="T">Type of the test instruction being executed.</typeparam>
		/// <returns>A task which will cause early failure or completion of the executed test instruction.</returns>
		Task<T> GetExternalResult<T>(CancellationToken cancellationToken);
	}
}
