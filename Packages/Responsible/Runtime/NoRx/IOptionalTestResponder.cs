using Responsible.NoRx.State;
using Responsible.NoRx.Utilities;

namespace Responsible.NoRx
{
	/// <summary>
	/// Represents one or more test responders, which can optionally execute when they are ready.
	///
	/// See <see cref="OptionalTestResponder"/> for extension methods on optional responders.
	/// </summary>
	public interface IOptionalTestResponder : ITestOperation<IMultipleTaskSource<ITestOperationState<Nothing>>>
	{
	}
}
