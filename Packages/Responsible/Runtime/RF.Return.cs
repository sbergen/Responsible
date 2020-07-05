using System;
using JetBrains.Annotations;
using Responsible.TestInstructions;

namespace Responsible
{
	// See RF.WaitFor.cs for documentation
	public static partial class RF
	{
		[Pure]
		public static ITestInstruction<T> Return<T>(T value) =>
			new FuncTestInstruction<T>(() => value);

		[Pure]
		public static ITestInstruction<T> ReturnDeferred<T>(Func<T> create) =>
			new FuncTestInstruction<T>(create);
	}
}