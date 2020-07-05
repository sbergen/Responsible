namespace Responsible
{
	/// <summary>
	/// This is really just an alias to make code look cleaner
	/// </summary>
	public interface ITestResponder<out T> : ITestWaitCondition<ITestInstruction<T>>
	{
	}
}