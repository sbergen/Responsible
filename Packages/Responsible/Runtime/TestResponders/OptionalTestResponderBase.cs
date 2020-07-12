using System;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	public class OptionalTestResponderBase : IOptionalTestResponder
	{
		private readonly Func<ITestOperationState<ITestOperationState<Unit>>> createState;

		protected OptionalTestResponderBase(Func<ITestOperationState<ITestOperationState<Unit>>> createState)
		{
			this.createState = createState;
		}

		public ITestOperationState<ITestOperationState<Unit>> CreateState() => this.createState();
	}
}