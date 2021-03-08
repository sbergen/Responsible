using System;
using Responsible.State;
using UniRx;

namespace Responsible.TestResponders
{
	internal class OptionalTestResponderBase : IOptionalTestResponder
	{
		private readonly Func<ITestOperationState<ITestOperationState<Unit>>> createState;

		protected OptionalTestResponderBase(Func<ITestOperationState<ITestOperationState<Unit>>> createState)
		{
			this.createState = createState;
		}

		public ITestOperationState<ITestOperationState<Unit>> CreateState() => this.createState();
	}
}
