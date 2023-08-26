using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.UnityTests
{
	public class SourcePrettifyTests : ResponsibleTestBase
	{
		[Test]
		public async Task SourcePath_IsPrettified_WhenInProject()
		{
			var task = Do("Throw", () => throw new Exception())
				.ToTask(this.Executor);
			var exception = await AwaitFailureExceptionForUnity(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("Throw")
				.Details("(at Assets/UnityTests/SourcePrettifyTests.cs:");
		}

		[Test]
		public async Task SourcePath_IsNotPrettified_WhenNotInProject()
		{
			var task = Do(
					"Throw",
					() => throw new Exception(),
					// ReSharper disable once ExplicitCallerInfoArgument
					sourceFilePath: "/foo/bar.cs")
				.ToTask(this.Executor);
			var exception = await AwaitFailureExceptionForUnity(task);
			StateAssert.StringContainsInOrder(exception.Message)
				.Failed("Throw")
				.Details("(at /foo/bar.cs:");
		}
	}
}
