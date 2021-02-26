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

		private static ITestWaitCondition<Unit> MakeOptionalResponder(ResponderState state) =>
			WaitForCondition(state.Condition1Name, () => state.Condition1)
				.ThenRespondWithAction(state.ResponseName, _ => state.ResponseAction())
				.Optionally()
				.Until(WaitForCondition(state.Condition2Name, () => state.Condition2));

		private static ITestResponder<Unit> MakeResponder(ResponderState state) =>
			WaitForCondition(state.Condition1Name, () => state.Condition1)
				.AndThen(_ => WaitForCondition(state.Condition2Name, () => state.Condition2))
				.ThenRespondWithAction(state.ResponseName, _ => state.ResponseAction());

		private static ITestInstruction<Unit> MakeInstruction(
			ResponderState state1,
			ResponderState state2,
			ResponderState state3) =>
			MakeOptionalResponder(state1)
				.ExpectWithinSeconds(10)
				.ContinueWith(
					RespondToAnyOf(MakeResponder(state2), MakeResponder(state3))
						.Until(WaitForCondition("Never", () => false))
						.ExpectWithinSeconds(10));

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
				};

				var state2 = new ResponderState
				{
					Condition1Name = "Cond 2.1",
					Condition2Name = "Cond 2.2",
					ResponseName = "Response 2",
					ResponseAction = () => throw new Exception("Exception"),
				};

				var state3 = new ResponderState
				{
					Condition1Name = "Cond 3.1",
					Condition2Name = "Cond 3.2",
					ResponseName = "Response 3",
				};

				MakeInstruction(state1, state2, state3)
					.ToObservable(executor)
					.CatchIgnore()
					.Subscribe();

				// Store logger output to variable, for easier setup (can be actually logged)
				string message = null;
				logger.Log(LogType.Error, Arg.Do<string>(msg => message = msg));

				// Advance time and frames, and complete condition that cancels the first wait
				scheduler.AdvanceBy(TimeSpan.FromMilliseconds(20));
				poll.OnNext(Unit.Default);
				state1.Condition2 = true;
				poll.OnNext(Unit.Default);

				// Advance time and frames, and complete one condition of second responder
				scheduler.AdvanceBy(TimeSpan.FromMilliseconds(20));
				poll.OnNext(Unit.Default);
				state2.Condition1 = true;
				poll.OnNext(Unit.Default);

				// Advance time and frames, and complete second condition of second responder
				scheduler.AdvanceBy(TimeSpan.FromMilliseconds(20));
				poll.OnNext(Unit.Default);
				state2.Condition2 = true;
				poll.OnNext(Unit.Default);

				// Second responder should trigger error, third one is not executed
				StringAssert.StartsWith(ExpectedOutput, message);
			}
		}

		private const string ExpectedOutput =
			@"Test operation execution failed!
 
Failure context:
[✓] EXPECT WITHIN 10.00 s (Completed in 0.02 s and 2 frames)
  UNTIL
    [✓] Cond 1.2 (Completed in 0.02 s and 2 frames)
  RESPOND TO ANY OF
    [-] Response 1 (Canceled after 0.02 s and 1 frames)
      WAIT FOR
        [-] Cond 1.1 (Canceled after 0.02 s and 1 frames)
      THEN RESPOND WITH ...
[!] EXPECT WITHIN 10.00 s (Failed after 0.04 s and 4 frames)
  UNTIL
    [.] Never (Started 0.04 s and 4 frames ago)
  RESPOND TO ANY OF
    [!] Response 2 (Failed after 0.00 s and 0 frames)
      WAIT FOR
        [✓] Cond 2.1 (Completed in 0.02 s and 2 frames)
        [.] Cond 2.2 (Started 0.02 s and 2 frames ago)
      THEN RESPOND WITH
        [!] Response 2 (Failed after 0.00 s and 0 frames)
 
          Failed with:
            System.Exception: 'Exception'
 
          Test operation stack:
            [ThenRespondWithAction] MakeResponder (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:35)
            [Until] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:45)
            [ExpectWithinSeconds] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:46)
            [ContinueWith] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:43)
            [ToObservable] ErrorOutput_IsAsExpected (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:86)
 
    [.] Response 3 (Started 0.04 s and 4 frames ago)
      WAIT FOR
        [.] Cond 3.1 (Started 0.04 s and 4 frames ago)
        [ ] ...
      THEN RESPOND WITH ...
 
Failed with:
  System.Exception: 'Exception'
 
Test operation stack:
  [ExpectWithinSeconds] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:46)
  [ContinueWith] MakeInstruction (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:43)
  [ToObservable] ErrorOutput_IsAsExpected (at Assets/Responsible/Tests/Runtime/ErrorOutputTests.cs:86)
 
 
Error: System.Exception: Exception";
	}
}