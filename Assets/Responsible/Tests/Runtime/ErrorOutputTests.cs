using System;
using NSubstitute;
using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using UniRx;
using UnityEngine;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class ErrorOutputTests
	{
		private class ResponderState
		{
			public string Condition1Name { get; set; }
			public bool Condition1 { get; set; }

			public string Condition2Name { get; set; }
			public bool Condition2 { get; set; }

			public string ResponseName { get; set; }

			public Action ResponseAction { get; set; }
		}

		private static ITestResponder<Unit> MakeResponder(ResponderState state) =>
			WaitForCondition(state.Condition1Name, () => state.Condition1)
				.AndThen(_ => WaitForCondition(state.Condition2Name, () => state.Condition2))
				.ThenRespondWith(state.ResponseName, _ => state.ResponseAction());

		private static ITestInstruction<Unit> MakeInstruction(ResponderState state1, ResponderState state2) =>
			RespondToAnyOf(MakeResponder(state1), MakeResponder(state2))
				.Until(WaitForCondition("Never", () => false))
				.ExpectWithinSeconds(10);

		[Test]
		public void ErrorOutput_IsAsExpected()
		{
			// Some aspects of the output is tested in other smaller tests, but
			// Having one sort of system test to assert the exact output is nice.
			// This could arguably be a bunch of smaller tests, also, but those
			// might become a pain to maintain, and the core functionality is
			// more important than the output details in various situations.
			// If issues with output arise, maybe this will be changed.
			// We especially do not want to assert things like line numbers in multiple places!
			var logger = Substitute.For<ILogger>();
			var scheduler = new TestScheduler();
			var poll = new Subject<Unit>();
			using (var executor = new TestInstructionExecutor(scheduler, poll, logger))
			{
				var state1 = new ResponderState
				{
					Condition1Name = "Cond 1.1",
					Condition2Name = "Cond 1.2",
					ResponseName = "Response 1",
					ResponseAction = () => throw new Exception("Exception"),
				};

				var state2 = new ResponderState
				{
					Condition1Name = "Cond 2.1",
					Condition2Name = "Cond 2.2",
					ResponseName = "Response 2",
				};

				MakeInstruction(state1, state2)
					.ToObservable(executor)
					.CatchIgnore()
					.Subscribe();

				// Store logger output to variable, for easier setup (can be actually logged)
				string message = null;
				logger.Log(LogType.Error, Arg.Do<string>(msg => message = msg));

				// Advance time and frames, and complete one condition
				scheduler.AdvanceBy(TimeSpan.FromSeconds(1.5));
				poll.OnNext(Unit.Default);
				state1.Condition1 = true;
				poll.OnNext(Unit.Default);

				// Advance time and frames, and complete second condition
				scheduler.AdvanceBy(TimeSpan.FromSeconds(1.5));
				poll.OnNext(Unit.Default);
				state1.Condition2 = true;
				poll.OnNext(Unit.Default);

				StringAssert.StartsWith(ExpectedOutput, message);
			}
		}

		private const string ExpectedOutput =
			@"Test operation execution failed!
 
Failure context:
[!] EXPECT WITHIN 0:00:10 (Failed after 3.00s and 4 frames)
  UNTIL
    [.] Never (Started 3.00s and 4 frames ago)
  RESPOND TO ANY OF
    [!] Response 1 (Failed after 0.00s and 0 frames)
      WAIT FOR
        [✓] Cond 1.1 (Completed in 1.50s and 2 frames)
        [.] Cond 1.2 (Started 1.50s and 2 frames ago)
      THEN RESPOND WITH
        [!] Response 1 (Failed after 0.00s and 0 frames)
 
          Failed with:
            System.Exception: 'Exception'
 
          Test operation stack:
            [ThenRespondWith] MakeResponder (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:29)
            [Until] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:33)
            [ExpectWithinSeconds] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:34)
            [ToObservable] ErrorOutput_IsAsExpected (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:67)
 
    [.] Response 2 (Started 3.00s and 4 frames ago)
      WAIT FOR
        [.] Cond 2.1 (Started 3.00s and 4 frames ago)
        [ ] ...
      THEN RESPOND WITH ...
 
Failed with:
  System.Exception: 'Exception'
 
Test operation stack:
  [ExpectWithinSeconds] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:34)
  [ToObservable] ErrorOutput_IsAsExpected (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:67)
 
 
Error: System.Exception: Exception";
	}
}