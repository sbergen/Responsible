namespace Responsible.Tests.Utilities
{
	public static class StateAssert
	{
		public static AssertStateString StringContainsInOrder(string str) => new AssertStateString(str);
	}
}
