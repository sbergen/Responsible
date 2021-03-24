using System.Runtime.CompilerServices;
using System.Threading;
using Responsible.Context;
using Responsible.Unity;

namespace Responsible
{
	public static partial class TestInstruction
	{
		/// <summary>
		/// Starts executing an instruction, and returns a yield instruction which can be awaited,
		/// using <c>yield return</c> in a Unity coroutine.
		/// </summary>
		/// <returns>Yield instruction for Unity, which will complete when the instruction has completed.</returns>
		/// <param name="instruction">Instruction to execute.</param>
		/// <typeparam name="T">Return type of the test instruction to start.</typeparam>
		/// <inheritdoc cref="Docs.Inherit.CallerMember{T1, T2}"/>
		public static TaskYieldInstruction<T> ToYieldInstruction<T>(
			this ITestInstruction<T> instruction,
			TestInstructionExecutor executor,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TaskYieldInstruction<T>(executor.RunInstruction(
				instruction.CreateState(),
				new SourceContext(nameof(ToYieldInstruction), memberName, sourceFilePath, sourceLineNumber),
				cancellationToken));
	}
}
