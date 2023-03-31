using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.EditorSetup
{
	public static class DocumentationExample
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

		public static async Task<string> CreateDocumentationFailure()
		{
			var foo = new Foo();
			var bar = new Bar();

			var scheduler = new MockTestScheduler();
			var task = WaitForCondition("Foo to be ready", () => foo.IsReady)
				.AndThen(WaitForCondition("Bar to be completed", () => bar.IsCompleted))
				.ThenRespondWith("Foo the bar", Do("Consume bar", () => foo.Consume(bar)))
				.ExpectWithinSeconds(10)
				.ContinueWith(Do("Continue operation", foo.ContinueOperation))
				.ToTask(new TestInstructionExecutor(scheduler));

			var oneFrame = TimeSpan.FromSeconds(1.0 / 60);
			scheduler.AdvanceFrame(oneFrame);
			scheduler.AdvanceFrame(oneFrame);

			foo.IsReady = true;

			scheduler.AdvanceFrame(oneFrame);
			scheduler.AdvanceFrame(oneFrame);
			scheduler.AdvanceFrame(oneFrame);
			scheduler.AdvanceFrame(oneFrame);

			bar.IsCompleted = true;

			scheduler.AdvanceFrame(oneFrame);

			Exception exception = null;
			try
			{
				await task;
			}
			catch (Exception e)
			{
				exception = e;
			}

			if (exception == null)
			{
				throw new InvalidOperationException("Expected test task to fail!");
			}

			var simpleReplacements = exception.Message
				.Replace(
					nameof(TestInstruction.ToTask),
					nameof(TestInstruction.ToYieldInstruction))
				.Replace(
					nameof(CreateDocumentationFailure),
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

			return $"```\n{stackTraceOmitted}\n```";
		}
	}
}
