using System;
using NSubstitute;
using NUnit.Framework;
using Responsible.Utilities;

namespace Responsible.Tests
{
	public class RetryingPollerTests
	{
		private RetryingPoller poller;

		[SetUp]
		public void SetUp()
		{
			this.poller = new RetryingPoller();
		}

		[Test]
		public void Poll_DoesNotThrow_WhenEmpty()
		{
			Assert.DoesNotThrow(this.poller.Poll);
		}

		[Test]
		public void Poll_CallsSingleCallbackOncePerCall()
		{
			var called = 0;
			this.poller.RegisterPollCallback(() => ++called);

			this.poller.Poll();
			Assert.AreEqual(1, called);

			this.poller.Poll();
			Assert.AreEqual(2, called);
		}

		[Test]
		public void DisposeHandle_StopsCallbacks()
		{
			var called = 0;
			this.poller.RegisterPollCallback(() => ++called).Dispose();

			this.poller.Poll();
			Assert.AreEqual(0, called);
		}

		[Test]
		public void AddingCallback_CausesSecondPoll()
		{
			var added = false;
			var callback1 = Substitute.For<Action>();
			var callback2 = Substitute.For<Action>();

			callback1
				.When(x => x.Invoke())
				.Do(_ =>
				{
					if (!added)
					{
						added = true;
						this.poller.RegisterPollCallback(callback2);
					}
				});

			this.poller.RegisterPollCallback(callback1);
			this.poller.Poll();

			Received.InOrder(() =>
			{
				callback1();
				callback2();
				callback1();
				callback2();
			});
		}

		[Test]
		public void RemovingCallback_CausesSecondPoll()
		{
			var callback1 = Substitute.For<Action>();
			var callback2 = Substitute.For<Action>();

			var handle = this.poller.RegisterPollCallback(callback1);
			this.poller.RegisterPollCallback(callback2);

			callback2
				.When(x => x.Invoke())
				.Do(_ =>
				{
					handle?.Dispose();
				});


			this.poller.Poll();

			Received.InOrder(() =>
			{
				callback1();
				callback2();
				callback2();
			});
		}
	}
}
