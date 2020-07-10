using System;
using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class DoTests : ResponsibleTestBase
	{
		[Test]
		public void Do_Executes_WithFunc()
		{
			var result = Do(() => 42).ToObservable().Wait(TimeSpan.Zero);
			Assert.AreEqual(42, result);
		}

		[Test]
		public void Do_Executes_WithAction()
		{
			var executed = false;
			Do(() => { executed = true; }).ToObservable().Wait(TimeSpan.Zero);
			Assert.IsTrue(executed);
		}

		[Test]
		public void Do_DoesNotExecute_UntilSubscribedTo()
		{
			var executed = false;
			var unused = Do(() => { executed = true; }).ToObservable();
			Assert.IsFalse(executed, "Instruction should not execute until subscribed to");
		}

		[Test]
		public void DoWithResult_ExecutesWithCorrectData()
		{
			int? result = null;
			Do(() => 2).ContinueWith(DoWithResult<int>(x => result = x * 2)).ToObservable().Wait(TimeSpan.Zero);
			Assert.AreEqual(4, result);
		}
	}
}