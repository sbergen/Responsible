using System;
using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		[Pure]
		public static ITestInstruction<Unit> RunCoroutine(
			string description,
			int timeoutSeconds,
			Func<IEnumerator> startCoroutine,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new CoroutineTestInstruction(
				startCoroutine,
				description,
				TimeSpan.FromSeconds(timeoutSeconds),
				new SourceContext(sourceFilePath, sourceLineNumber));
	}
}