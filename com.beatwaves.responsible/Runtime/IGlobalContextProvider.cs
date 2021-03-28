using Responsible.State;

namespace Responsible
{
	/// <summary>
	/// Interface for providing global context to failure messages.
	/// </summary>
	public interface IGlobalContextProvider
	{
		/// <summary>
		/// Add any details that might be useful as global context to include when test operations fail.
		/// </summary>
		/// <param name="contextBuilder">The string builder to use to build the context.</param>
		void BuildGlobalContext(StateStringBuilder contextBuilder);
	}
}
