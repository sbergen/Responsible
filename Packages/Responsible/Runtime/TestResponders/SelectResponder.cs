using System;
using Responsible.Context;
using Responsible.State;

namespace Responsible.TestResponders
{
	internal class SelectResponder<T1, T2> : TestResponderBase<T2>
	{
		public SelectResponder(ITestResponder<T1> responder, Func<T1, T2> selector, SourceContext sourceContext)
			: base(() => new SelectOperationState<IOperationState<T1>, IOperationState<T2>>(
				responder.CreateState(),
				state => new SelectOperationState<T1, T2>(state, selector, sourceContext),
				sourceContext))
		{
		}
	}
}