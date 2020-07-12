using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using Responsible.Utilities;
using UniRx;

namespace Responsible
{
	// See Responsibly.WaitFor.cs for documentation
	public static partial class Responsibly
	{
		[Pure]
		public static ITestInstruction<T> Do<T>(
			string description,
			Func<T> func,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(
				description,
				func,
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestInstruction<Unit> Do(
			string description,
			Action action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<Unit>(
				description,
				action.AsUnitFunc(),
				new SourceContext(memberName, sourceFilePath, sourceLineNumber));
	}
}