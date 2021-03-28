using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Responsible.Context;
using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestOperationState{T}"/>.
	/// These are normally not needed, but can be useful for debugging purposes.
	/// </summary>
	// ReSharper disable once PartialTypeWithSinglePart, has a Unity part
	public static partial class TestOperationState
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
		/// Type inference to hack around lack of covariant generic classes in C#.
		/// </summary>
		internal static Task<T> Execute<T>(
			this ITestOperationState<T> state,
			RunContext runContext,
			CancellationToken cancellationToken)
			=> state.ExecuteUnsafe<T>(runContext, cancellationToken);

		// This is needed in some internals, so for convenience, we don't constrain it to structs
		internal static ITestOperationState<object> BoxResult<T>(this ITestOperationState<T> state) =>
			typeof(T).IsClass
				? (ITestOperationState<object>)state
				: new BoxedOperationState<T, object>(state, value => (object)value);
	}
}
