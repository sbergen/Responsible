using System.Runtime.CompilerServices;
using System.Threading;
using Responsible.Context;
using Responsible.State;
using Responsible.Unity;

namespace Responsible
{
	public static partial class TestOperationState
	{
		/// <summary>
		/// **Unity-only!**
		///
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
		/// <inheritdoc cref="Docs.Inherit.YieldInstruction{T}"/>
		public static TestOperationYieldInstruction<T> ToYieldInstruction<T>(
			this ITestOperationState<T> state,
			TestInstructionExecutor executor,
			bool throwOnError = true,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TestOperationYieldInstruction<T>(executor.RunInstruction(
					state,
					new SourceContext(nameof(ToYieldInstruction), memberName, sourceFilePath, sourceLineNumber),
					cancellationToken),
				throwOnError);
	}
}
