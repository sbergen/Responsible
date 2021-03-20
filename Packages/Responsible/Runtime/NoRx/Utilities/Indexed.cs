namespace Responsible.NoRx.Utilities
{
	internal readonly struct Indexed<T>
	{
		public readonly T Value;
		public readonly int Index;

		public Indexed(T value, int index)
		{
			this.Value = value;
			this.Index = index;
		}
	}

	internal static class Indexed
	{
		public static Indexed<T> Make<T>(T value, int index)
			=> new Indexed<T>(value, index);

		public static T[] AssignToArray<T>(T[] array, Indexed<T> indexed)
		{
			array[indexed.Index] = indexed.Value;
			return array;
		}
	}
}
