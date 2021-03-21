using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Responsible.NoRx.Context;
using Responsible.NoRx.State;

namespace Responsible.NoRx
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestOperationState"/>.
	/// These are normally not needed, but can be useful for debugging purposes.
	/// </summary>
	public static class TestOperationState
	{
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

		internal static ITestOperationState<Nothing> AsNothingOperationState<T>(this ITestOperationState<T> state)
			=> new NothingOperationState<T, Nothing>(state, _ => Nothing.Default);
	}
}
