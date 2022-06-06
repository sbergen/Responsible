using System;

namespace Responsible
{
	/// <summary>
	/// The exception type that will be thrown if the maximum number of repetitions of
	/// a repeated operation is exceeded.
	/// </summary>
	public sealed class RepetitionLimitExceededException : Exception
	{
		internal RepetitionLimitExceededException(int repetitionLimit)
			: base($"Exceeded the maximum number of repetitions ({repetitionLimit})")
		{
		}
	}
}
