using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Responsible.Context;
using Responsible.TestInstructions;
using UniRx;

// ReSharper disable ExplicitCallerInfoArgument

namespace Responsible
{
	// See RF.WaitFor.cs for documentation
	public static partial class RF
	{
		[Pure]
		public static ITestInstruction<T> Do<T>(
			Func<T> func,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<T>(func, new SourceContext(sourceFilePath, sourceLineNumber));

		[Pure]
		public static ITestInstruction<Unit> Do(
			Action action,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new SynchronousTestInstruction<Unit>(
				() =>
				{
					action();
					return Unit.Default;
				},
				new SourceContext(sourceFilePath, sourceLineNumber));

		[Pure]
		public static Func<TResult, ITestInstruction<Unit>> DoWithResult<TResult>(
			Action<TResult> action,
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			result => Do(() => action(result), sourceFilePath, sourceLineNumber);
	}
}