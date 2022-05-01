using System;
using Responsible.State;
using Responsible.Utilities;

namespace Responsible.TestResponders
{
	internal class OptionalTestResponderBase : IOptionalTestResponder
	{
		private readonly Func<ITestOperationState<IMultipleTaskSource<ITestOperationState<object?>>>> createState;

		protected OptionalTestResponderBase(
			Func<ITestOperationState<IMultipleTaskSource<ITestOperationState<object?>>>> createState)
		{
			this.createState = createState;
		}

		public ITestOperationState<IMultipleTaskSource<ITestOperationState<object?>>> CreateState() =>
			this.createState();
	}
}
