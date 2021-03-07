namespace Responsible.State
{
	public abstract class TestOperationStateNotification
	{
		public readonly ITestOperationState State;

		protected TestOperationStateNotification(ITestOperationState state)
		{
			this.State = state;
		}

		public class Started : TestOperationStateNotification
		{
			public Started(ITestOperationState state)
				: base(state)
			{
			}
		}

		public class Finished : TestOperationStateNotification
		{
			public Finished(ITestOperationState state)
				: base(state)
			{
			}
		}
	}
}
