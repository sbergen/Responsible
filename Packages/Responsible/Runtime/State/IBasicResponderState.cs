using JetBrains.Annotations;

namespace Responsible.State
{
	/// <summary>
	/// Operation state for a basic responder, the description of which can be inlined
	/// with an expect within operation.
	/// </summary>
	internal interface IBasicResponderState : ITestOperationState
	{
		string Description { get; }
		ITestOperationState WaitState { get; }
		[CanBeNull] ITestOperationState InstructionState { get; }
	}
}