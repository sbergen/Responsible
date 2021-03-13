using Responsible.State;
using UniRx;

namespace Responsible
{
	/// <summary>
	/// Represents one or more test responders, which can optionally execute when they are ready.
	///
	/// See <see cref="OptionalTestResponder"/> for extension methods on optional responders.
	/// </summary>
	public interface IOptionalTestResponder : ITestOperation<ITestOperationState<Unit>>
	{
	}
}
