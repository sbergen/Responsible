namespace Responsible.Utilities
{
	internal class Unit
	{
		private Unit()
		{
		}

		public static object Instance => new Unit();
	}
}
