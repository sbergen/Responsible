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
		/// Start executing from explicitly created state. This can be used for e.g. logging the state periodically.
		/// </summary>
		/// <remarks>
		/// Be careful not to call this twice on the same state object: the consequences are unknown.
		/// </remarks>
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
		/// Start executing from explicitly created state. This can be used for e.g. logging the state periodically.
		/// </summary>
		/// <remarks>
		/// Be careful not to call this twice on the same state object: the consequences are unknown.
		/// </remarks>
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