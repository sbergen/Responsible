namespace Responsible.State
{
	internal abstract class ContinuationState
	{
		public class Available : ContinuationState
		{
			public readonly ITestOperationState State;

			public Available(ITestOperationState state)
			{
				this.State = state;
			}
		}

		public class NotAvailable : ContinuationState
		{
			public readonly TestOperationStatus CreationStatus;

			public NotAvailable(TestOperationStatus creationStatus)
			{
				this.CreationStatus = creationStatus;
			}
		}
	}
}
