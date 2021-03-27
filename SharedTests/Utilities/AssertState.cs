namespace Responsible.Tests.Utilities
{
	public static class AssertState
	{
		public static AssertStateString StringContains(string str) => new AssertStateString(str);
	}
}
