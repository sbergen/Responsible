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
		public void ExceptionMessage_IncludesAllDetails()
		{
			this.GlobalContextProvider.BuildGlobalContext(Arg.Do<StateStringBuilder>(
				b => b.AddDetails("Global details")));

			var task = WaitForCondition(
					"Throw",
					() => throw new Exception("An exception"),
					builder => builder.AddDetails("Local details"))
				.ExpectWithinSeconds(1)
				.ToTask(this.Executor);
			var failureException = GetFailureException(task);

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
		public void ExceptionMessage_DoesNotIncludeCompletelyEmptyLines()
		{
			var task = Do("Throw", () => throw new Exception())
				.ToTask(this.Executor);
			var lines = GetFailureException(task).Message.Split('\n');
			CollectionAssert.Contains(lines, " ");
			CollectionAssert.DoesNotContain(lines, "");
		}

		[Test]
		public void OperationStack_ContainsSourceOnlyOnce_WithSequence()
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

			ExpectOperatorCountInError(task, "Sequence", 1);
		}

		[Test]
		public void OperationStack_ContainsSourceTwice_WithMultipleOperatorsOnSameLine()
		{
			var throwInstruction = Do("Throw error", () => throw new Exception());
			var task = Return(1).ContinueWith(throwInstruction).ContinueWith(Return(3))
				.ToTask(this.Executor);

			ExpectOperatorCountInError(task, "ContinueWith", 2);
		}

		[AssertionMethod]
		private static void ExpectOperatorCountInError(Task task, string operatorName, int count)
		{
			var message = GetFailureException(task).Message;
			var sequenceCount = Regex.Matches(message, $@"\[{operatorName}\]").Count;
			Assert.AreEqual(count, sequenceCount, $"[{operatorName}] should occur {count} time(s) in the error message: {message}");
		}
	}
}
