using System;
using Responsible.TestInstructions;

namespace Responsible
{
	public static class TestInstruction
	{
		public static ITestInstruction<T> Return<T>(T value) =>
			new FuncTestInstruction<T>(() => value);

		public static ITestInstruction<T> Defer<T>(Func<T> create) =>
			new FuncTestInstruction<T>(create);
	}
}