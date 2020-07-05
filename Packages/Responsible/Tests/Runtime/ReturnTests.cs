using System;
using NUnit.Framework;
using UniRx;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class ReturnTests
	{
		[Test]
		public void Return_ReturnsCorrectValue()
		{
			var value = 42;
			var returnInstruction = Return(value);
			Assert.AreEqual(42, returnInstruction.Execute().Wait(TimeSpan.Zero));
		}

		[Test]
		public void ReturnDeferred_ReturnsCorrectValue()
		{
			var value = 42;
			var returnInstruction = ReturnDeferred(() => value);
			Assert.AreEqual(42, returnInstruction.Execute().Wait(TimeSpan.Zero));

			++value;
			Assert.AreEqual(43, returnInstruction.Execute().Wait(TimeSpan.Zero));
		}
	}
}