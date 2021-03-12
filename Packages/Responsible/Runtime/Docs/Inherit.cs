using System.Diagnostics.CodeAnalysis;

namespace Responsible.Docs
{
	/// <summary>
	/// Class to help generate documentation with DocFX with inheritdoc tags.
	/// Not to be used in actual code!
	/// </summary>
	/// <remarks>
	/// Can't be excluded from DocFX or be internal without breaking functionality :/
	/// DocFX seems to require the argument count to match, thus this is a bit repetitive.
	/// </remarks>
	[ExcludeFromCodeCoverage]
	public abstract class Inherit
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
		public void CallerMember<T1>(
			T1 arg1,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}

		/// <param name="memberName">
		/// Caller member name provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceFilePath">
		/// Caller file path provided by compiler. May be overridden for custom operators.
		/// </param>
		/// <param name="sourceLineNumber">
		/// Source line number provided by compiler. May be overridden for custom operators.
		/// </param>
		public void CallerMember<T1, T2>(
			T1 arg1,
			T2 arg2,
			string memberName = "",
			string sourceFilePath = "",
			int sourceLineNumber = 0)
		{
		}
	}
}
