using System.Runtime.CompilerServices;

namespace Responsible.Docs
{
	/// <summary>
	/// Class to help generate documentation. Has to be public to work...
	/// </summary>
	/// <remarks>
	/// Can't be excluded from DocFX without breaking functionality :/
	/// </remarks>
	public static class Provider
	{
		/// <param name="memberName">
		/// Caller member name provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceFilePath">
		/// Caller file path provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceLineNumber">
		/// Source line number provided by compiler. May be overridden for custom operators.
		/// </param>
		public static void CallerMember<T1, T2>(
			T1 arg1,
			T2 arg2,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
		}
	}
}
