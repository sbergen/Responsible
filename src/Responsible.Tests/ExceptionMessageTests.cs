using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;
using Responsible.State;
using Responsible.Tests.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests
{
	public class ExceptionMessageTests : ResponsibleTestBase
	{
		protected override IGlobalContextProvider MakeGlobalContextProvider() =>
			Substitute.For<IGlobalContextProvider>();

		[Test]
		public async Task ExceptionMessage_IncludesAllDetails()
		{
			this.GlobalContextProvider.BuildGlobalContext(Arg.Do<StateStringBuilder>(
				b => b.AddDetails("Global details")));

			var task = WaitForCondition(
					"Throw",
					() => throw new Exception("An exception"),
					builder => builder.AddDetails("Local details"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
			var failureException = await AwaitFailureExceptionForUnity(task);

			StateAssert
				.StringContainsInOrder(failureException.Message)
				.Details("Test operation execution failed")
				.Failed("Throw")
				.EmptyLine()
				.Details("Failed with:")
				.Details("An exception")
				.EmptyLine()
				.Details("Test operation stack:")
				.Details(@"\[ExpectWithinSeconds\].*?\(at")
				.Details(@"\[ToTask\].*?\(at")
				.EmptyLine()
				.Details("Local details")
				.EmptyLine()
				.Details("Global details")
				.EmptyLine()
				.Details("Error:");
		}

		[Test]
		public async Task ExceptionMessage_DoesNotIncludeCompletelyEmptyLines()
		{
			var task = Do("Throw", () => throw new Exception())
				.ToTask(this.Executor);
			var lines = (await AwaitFailureExceptionForUnity(task)).Message.Split('\n');
			CollectionAssert.Contains(lines, " ");
			CollectionAssert.DoesNotContain(lines, "");
		}

		[Test]
		public async Task OperationStack_ContainsSourceOnlyOnce_WithSequence()
		{
			var task = TestInstruction
				.Sequence(new[]
				{
					Return((object)1),
					Return((object)2),
					Do("Throw error", () => throw new Exception()),
					Return((object)3),
					Return((object)4),
				})
				.ToTask(this.Executor);

			await ExpectOperatorCountInError(task, "Sequence", 1);
		}

		[Test]
		public async Task OperationStack_ContainsSourceTwice_WithMultipleOperatorsOnSameLine()
		{
			var throwInstruction = Do("Throw error", () => throw new Exception());
			var task = Return(1).ContinueWith(throwInstruction).ContinueWith(Return(3))
				.ToTask(this.Executor);

			await ExpectOperatorCountInError(task, "ContinueWith", 2);
		}

		[AssertionMethod]
		private static async Task ExpectOperatorCountInError(Task task, string operatorName, int count)
		{
			var message = (await AwaitFailureExceptionForUnity(task)).Message;
			var sequenceCount = Regex.Matches(message, $@"\[{operatorName}\]").Count;
			Assert.AreEqual(count, sequenceCount, $"[{operatorName}] should occur {count} time(s) in the error message: {message}");
		}
	}
}
