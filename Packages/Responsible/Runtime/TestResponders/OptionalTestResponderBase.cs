using System;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	public class OptionalTestResponderBase : IOptionalTestResponder
	{
		private readonly Func<IOperationState<IOperationState<Unit>>> createState;

		protected OptionalTestResponderBase(Func<IOperationState<IOperationState<Unit>>> createState)
		{
			this.createState = createState;
		}

		public IOperationState<IOperationState<Unit>> CreateState() => this.createState();
	}
}