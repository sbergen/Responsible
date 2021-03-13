using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.State;
using UniRx;

namespace Responsible
{
	public static class TestOperationState
	{
		/// <summary>
		/// Construct an observable, which will start executing the explicitly created operation state.
		/// This can be used for e.g. logging the state periodically.
		/// </summary>
		/// <remarks>
		/// Be careful not to call this twice on the same state object: the consequences are undefined.
		/// </remarks>
		/// <returns>
		/// An observable which will complete with the value of the test operations, once it has completed,
		/// or publish an error if the operation fails.
		/// </returns>
		/// <param name="state">Test operation state to start executing.</param>
		/// <typeparam name="T">Result type of the test operation.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithExecutor{T}"/>
		[Pure]
		public static IObservable<T> ToObservable<T>(
			this ITestOperationState<T> state,
			TestInstructionExecutor executor,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				state,
				new SourceContext(nameof(ToObservable), memberName, sourceFilePath, sourceLineNumber));

		/// <summary>
		/// Start executing an explicitly created operation state.
		/// This can be used for e.g. logging the state periodically.
		/// </summary>
		/// <remarks>
		/// Be careful not to call this twice on the same state object: the consequences are undefined.
		/// </remarks>
		/// <returns>
		/// An yield instruction which will complete with the value of the test operations, once it has completed,
		/// or publish an error if the operation fails.
		/// </returns>
		/// <param name="state">Test operation state to start executing.</param>
		/// <typeparam name="T">Result type of the test operation.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMemberWithExecutor{T}"/>
		[Pure]
		public static ObservableYieldInstruction<T> ToYieldInstruction<T>(
			this ITestOperationState<T> state,
			TestInstructionExecutor executor,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
					state,
					new SourceContext(nameof(ToYieldInstruction), memberName, sourceFilePath, sourceLineNumber))
				.ToYieldInstruction();

		internal static ITestOperationState<Unit> AsUnitOperationState<T>(this ITestOperationState<T> state)
			=> new UnitOperationState<T, Unit>(state, _ => Unit.Default);
	}
}
