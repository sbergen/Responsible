using System;
using Responsible.NoRx.State;
using Responsible.NoRx.Utilities;

namespace Responsible.NoRx.TestResponders
{
	internal class OptionalTestResponderBase : IOptionalTestResponder
	{
		private readonly Func<ITestOperationState<IMultipleTaskSource<ITestOperationState<Nothing>>>> createState;

		protected OptionalTestResponderBase(
			Func<ITestOperationState<IMultipleTaskSource<ITestOperationState<Nothing>>>> createState)
		{
			this.createState = createState;
		}

		public ITestOperationState<IMultipleTaskSource<ITestOperationState<Nothing>>> CreateState() =>
			this.createState();
	}
}
