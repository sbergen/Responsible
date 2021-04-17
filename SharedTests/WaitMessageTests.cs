using System;
using NUnit.Framework;
using Responsible.Tests.Utilities;

namespace Responsible.Tests
{
	public class WaitMessageTests : ResponsibleTestBase
	{
		[Test]
		public void WaitMessage_ContainsCorrectDetails()
		{
			var state = Never.ExpectWithinSeconds(1).CreateState();

			this.AdvanceDefaultFrame();

			state.ToTask(this.Executor); // Start execution

			StateAssert.StringContainsInOrder(state.ToString())
				.Waiting("Never")
				.Details(@"Started 0\.00 s and 0 frames ago");

			this.AdvanceDefaultFrame();
			StateAssert.StringContainsInOrder(state.ToString())
				.Waiting("Never")
				.Details($@"Started {OneFrame.TotalSeconds:0.00} s and 1 frames ago");
		}

	}
}
