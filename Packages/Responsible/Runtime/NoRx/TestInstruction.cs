using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Responsible.NoRx.Context;

namespace Responsible.NoRx
{
	/// <summary>
	/// Contains extension methods on <see cref="ITestInstruction{T}"/>,
	/// for executing, sequencing and transforming their results.
	/// </summary>
	public static class TestInstruction
	{
		[Pure]
		public static Task<T> ToTask<T>(
			this ITestInstruction<T> instruction,
			TestInstructionExecutor executor,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> executor.RunInstruction(
				instruction.CreateState(),
				new SourceContext(nameof(ToTask), memberName, sourceFilePath, sourceLineNumber));
	}
}
