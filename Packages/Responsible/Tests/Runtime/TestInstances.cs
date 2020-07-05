using System;
using UniRx;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	internal static class TestInstances
	{
		public static readonly ITestWaitCondition<bool> ImmediateTrue =
			WaitForCondition("True", () => true, () => true);

		public static readonly ITestWaitCondition<Unit> Never =
			WaitForCondition("Never", () => false);

		public static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

	}
}