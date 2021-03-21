using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;
using Responsible.Unity;

namespace Responsible
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestOperationState"/>.
	/// These are normally not needed, but can be useful for debugging purposes.
	/// </summary>
	public static class TestOperationState
	{
		/// <summary>
		/// Starts executing the explicitly created operation state.
		/// This can be used for e.g. logging the state periodically.
		/// </summary>
		/// <remarks>
		/// Be careful not to call this twice on the same state object: the consequences are undefined.
		/// </remarks>
		/// <returns>
		/// An task which will complete with the value of the test operation, once it has completed,
		/// or an error if the operation fails.
		/// </returns>
		/// <param name="state">Test operation state to start executing.</param>
		/// <typeparam name="T">Result type of the test operation.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithExecutor{T}"/>
		public static Task<T> ToTask<T>(
			this ITestOperationState<T> state,
			TestInstructionExecutor executor,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				state,
				new SourceContext(nameof(ToTask), memberName, sourceFilePath, sourceLineNumber),
				cancellationToken);

		/// <summary>
		/// Start executing an explicitly created operation state.
		/// This can be used for e.g. logging the state periodically.
		/// </summary>
		/// <remarks>
		/// Be careful not to call this twice on the same state object: the consequences are undefined.
		/// </remarks>
		/// <returns>
		/// A yield instruction which will complete with the value of the test operation, once it has completed,
		/// or publish an error if the operation fails.
		/// </returns>
		/// <param name="state">Test operation state to start executing.</param>
		/// <typeparam name="T">Result type of the test operation.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithExecutor{T}"/>
		public static TaskYieldInstruction<T> ToYieldInstruction<T>(
			this ITestOperationState<T> state,
			TestInstructionExecutor executor,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TaskYieldInstruction<T>(executor.RunInstruction(
				state,
				new SourceContext(nameof(ToYieldInstruction), memberName, sourceFilePath, sourceLineNumber),
				cancellationToken));

		internal static ITestOperationState<Nothing> AsNothingOperationState<T>(this ITestOperationState<T> state)
			=> new NothingOperationState<T, Nothing>(state, _ => Nothing.Default);
	}
}
