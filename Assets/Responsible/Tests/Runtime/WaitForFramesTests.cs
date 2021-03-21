using NUnit.Framework;
using Responsible.NoRx;
using static Responsible.NoRx.Responsibly;

namespace Responsible.Tests.Runtime.NoRx
{
	public class WaitForFramesTests : ResponsibleTestBase
	{
		[Test]
		public void WaitForFrames_CompletesAfterTimeout()
		{
			var task = WaitForFrames(2).ToTask(this.Executor);

			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame(); // This frame
			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame(); // First frame
			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame(); // Second frame
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void WaitForFrames_CompletesAfterThisFrame_WithZeroFrames()
		{
			var task = WaitForFrames(0).ToTask(this.Executor);
			Assert.IsFalse(task.IsCompleted);
			this.AdvanceDefaultFrame(); // This frame
			Assert.IsTrue(task.IsCompleted);
		}

		[Test]
		public void WaitForFrames_ContainsCorrectStatusInDescription()
		{
			var state = WaitForFrames(0).CreateState();
			StringAssert.Contains("[ ]", state.ToString());
			state.ToTask(this.Executor);
			StringAssert.Contains("[.]", state.ToString());
			this.AdvanceDefaultFrame();
			StringAssert.Contains("[✓]", state.ToString());
		}
	}
}
