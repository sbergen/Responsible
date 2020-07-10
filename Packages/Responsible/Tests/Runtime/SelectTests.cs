using System;
using NUnit.Framework;
using UniRx;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class SelectTests : ResponsibleTestBase
	{
		[Test]
		public void Select_GetApplied_WhenSuccessful()
		{
			var result = Do(() => 2)
				.Select(val => val * 2)
				.ToObservable()
				.Wait();
			Assert.AreEqual(4, result);
		}

		[Test]
		public void Select_PublishesCorrectError_WhenExceptionThrown()
		{
			var observable = Do(() => 2)
				.Select<int, int>(_ => throw new Exception("Fail!"))
				.ToObservable();
			Assert.Throws<AssertionException>(() => observable.Wait());
		}
	}
}