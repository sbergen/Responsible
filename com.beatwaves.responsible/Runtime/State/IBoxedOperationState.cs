namespace Responsible.State
{
	internal interface IBoxedOperationState
	{
		ITestOperationState WrappedState { get; }
	}
}
