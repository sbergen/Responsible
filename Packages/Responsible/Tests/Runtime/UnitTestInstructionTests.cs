using System;
using NUnit.Framework;
using UniRx;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class UnitTestInstructionTests : ResponsibleTestBase
	{
		[Test]
		public void UnitTestInstruction_DoesNotThrow()
		{
			Assert.AreEqual(
				Unit.Default,
				Return(42).AsUnitInstruction().Execute().Wait(TimeSpan.Zero));
		}
	}
}