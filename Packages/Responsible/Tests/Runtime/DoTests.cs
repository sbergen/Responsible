using System;
using NUnit.Framework;
using UniRx;
using static Responsible.RF;

namespace Responsible.Tests.Runtime
{
	public class DoTests : ResponsibleTestBase
	{
		[Test]
		public void Do_Executes_WithFunc()
		{
			var result = Do(() => 42).Execute().Wait(TimeSpan.Zero);
			Assert.AreEqual(42, result);
		}

		[Test]
		public void Do_Executes_WithAction()
		{
			var executed = false;
			Do(() => { executed = true; }).Execute().Wait(TimeSpan.Zero);
			Assert.IsTrue(executed);
		}

		[Test]
		public void DoWithResult_ExecutesWithCorrectData()
		{
			int? result = null;
			Do(() => 2).ContinueWith(DoWithResult<int>(x => result = x * 2)).Execute().Wait(TimeSpan.Zero);
			Assert.AreEqual(4, result);
		}
	}
}