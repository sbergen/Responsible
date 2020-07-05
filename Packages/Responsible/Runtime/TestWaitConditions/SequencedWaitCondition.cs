using System;
using JetBrains.Annotations;
using Responsible.Context;
using UniRx;

namespace Responsible.TestWaitConditions
{
	internal class SequencedWaitCondition<TFirst, TSecond> : ITestWaitCondition<TSecond>
	{
		private readonly ITestWaitCondition<TFirst> first;
		[CanBeNull] private readonly Func<TFirst, ITestWaitCondition<TSecond>> continuation;
		[CanBeNull] private ITestWaitCondition<TSecond> latestSecond;

		public SequencedWaitCondition(
			ITestWaitCondition<TFirst> first, Func<TFirst, ITestWaitCondition<TSecond>> continuation)
		{
			this.first = first;
			this.continuation = continuation;
		}

		public SequencedWaitCondition(
			ITestWaitCondition<TFirst> first, ITestWaitCondition<TSecond> second)
		{
			this.first = first;
			this.latestSecond = second;
		}

		public IObservable<TSecond> WaitForResult(RunContext runContext, WaitContext waitContext) => this.first
			.WaitForResult(runContext, waitContext)
			.ContinueWith(result =>
			{
				if (this.continuation != null)
				{
					this.latestSecond = this.continuation(result);
					if (this.latestSecond == null)
					{
						throw new InvalidOperationException(
							$"Continuation on {this.GetType()} returned null");
					}
				}
				else if (this.latestSecond == null)
				{
					throw new InvalidOperationException(
						$"Constructed {this.GetType()} with neither continuation nor second condition");
				}

				return this.latestSecond.WaitForResult(runContext, waitContext);
			});

		public void BuildDescription(ContextStringBuilder builder)
		{
			builder.Add("FIRST", this.first);
			if (this.latestSecond != null)
			{
				builder.Add("AND THEN",  this.latestSecond);
			}
			else
			{
				builder.Add("AND THEN (not available)");
			}
		}

		public void BuildFailureContext(ContextStringBuilder builder) => this.BuildDescription(builder);
	}
}