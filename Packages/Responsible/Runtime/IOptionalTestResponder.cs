using Responsible.State;
using UniRx;

namespace Responsible
{
	/// <summary>
	/// Represents one or more test responders, which can optionally execute when they are ready.
	/// </summary>
	public interface IOptionalTestResponder : ITestOperation<ITestOperationState<Unit>>
	{
	}
}