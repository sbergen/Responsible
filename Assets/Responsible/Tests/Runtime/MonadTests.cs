using NUnit.Framework;
using Responsible.Tests.Runtime.Utilities;
using static Responsible.Responsibly;

namespace Responsible.Tests.Runtime
{
	public class MonadTests : ResponsibleTestBase
	{
		// These tests don't really prove anything, but are a nice exercise :)
		// They miss the "for all" aspect...

		[Test]
		public void LeftIdentity()
		{
			ITestInstruction<int> MultiplyByTwo(int value) => Return(2 * value);
			this.AssertEqual(
				Return(2).ContinueWith(MultiplyByTwo),
				MultiplyByTwo(2));
		}

		[Test]
		public void RightIdentity()
		{
			// As we capture caller context in Return, can't just use Return<int> here :/
			this.AssertEqual(
				Return(2).ContinueWith(val => Return(val)),
				Return(2));
		}

		[Test]
		public void Associativity()
		{
			ITestInstruction<int> MultiplyByTwo(int value) => Return(2 * value);
			ITestInstruction<int> AddOne(int value) => Return(value + 1);

			this.AssertEqual(
				Return(2).ContinueWith(MultiplyByTwo).ContinueWith(AddOne),
				Return(2).ContinueWith(x => MultiplyByTwo(x).ContinueWith(AddOne)));
		}

		private void AssertEqual<T>(ITestInstruction<T> left, ITestInstruction<T> right)
			=> Assert.AreEqual(
				left.ToTask(this.Executor).AssertSynchronousResult(),
				right.ToTask(this.Executor).AssertSynchronousResult());
	}
}
