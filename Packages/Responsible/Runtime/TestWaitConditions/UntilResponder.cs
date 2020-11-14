using Responsible.Context;
using Responsible.State;

namespace Responsible.TestWaitConditions
{
	internal class UntilResponder<T> : TestWaitConditionBase<T>
	{
		public UntilResponder(
			IOptionalTestResponder responder,
			ITestWaitCondition<T> condition,
			SourceContext sourceContext)
			: base(() => new UntilResponderState<T>(
				"UNTIL",
				responder.CreateState(),
				condition.CreateState(),
				sourceContext))
		{
		}
	}
}