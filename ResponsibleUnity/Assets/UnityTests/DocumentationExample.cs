using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Responsible.Tests;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.UnityTests
{
	public class DocumentationExample : ResponsibleTestBase
	{
		class Foo
		{
			public bool IsReady { get; set; }

			public void Consume(Bar bar)
			{
				var unused = bar;
				throw new Exception("Something failed");
			}

			public void ContinueOperation()
			{
			}
		}

		class Bar
		{
			public bool IsCompleted { get; set; }
		}

		[Test]
		public async Task CreateDocumentationFailure()
		{
			var foo = new Foo();
			var bar = new Bar();

			var task = WaitForCondition("Foo to be ready", () => foo.IsReady)
				.AndThen(WaitForCondition("Bar to be completed", () => bar.IsCompleted))
				.ThenRespondWith("Foo the bar", Do("Consume bar", () => foo.Consume(bar)))
				.ExpectWithinSeconds(10)
				.ContinueWith(Do("Continue operation", foo.ContinueOperation))
				.ToTask(this.Executor);

			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();

			foo.IsReady = true;

			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();
			this.AdvanceDefaultFrame();

			bar.IsCompleted = true;

			this.AdvanceDefaultFrame();

			var error = await AwaitFailureExceptionForUnity(task);
			var simpleReplacements = error.Message
				.Replace(
					nameof(TestInstruction.ToTask),
					nameof(TestInstruction.ToYieldInstruction))
				.Replace(
					nameof(this.CreateDocumentationFailure),
					"MethodName");
			var sourceReplaced = Regex.Replace(
				simpleReplacements,
				@"\(at .*?\.cs:(\d+)\)",
				match => $"(at Path/To/Source.cs:{match.Groups[1].Value})");
			var stackTraceOmitted = Regex.Replace(
				sourceReplaced,
				@"System.Exception: Something failed.*",
				"System.Exception: Something failed\n  at <normal exception stack trace comes here>",
				RegexOptions.Singleline);
			Debug.Log(stackTraceOmitted);
		}
	}
}
