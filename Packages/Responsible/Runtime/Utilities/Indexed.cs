using System;
using UniRx;

namespace Responsible.Utilities
{
	public readonly struct Indexed<T>
	{
		public readonly T Value;
		public readonly int Index;

		public Indexed(T value, int index)
		{
			this.Value = value;
			this.Index = index;
		}
	}

	public static class Indexed
	{
		public static Indexed<T> Make<T>(T value, int index)
			=> new Indexed<T>(value, index);

		public static IObservable<Indexed<T>> WithIndex<T>(
			this IObservable<T> source,
			int index)
			=> source.Select(value => Make(value, index));

		public static IObservable<Indexed<T>> WithIndexFrom<T, TIndexed>(
			this IObservable<T> source,
			Indexed<TIndexed> indexed)
			=> source.Select(value => Make(value, indexed.Index));

		public static T[] AssignToArray<T>(T[] array, Indexed<T> indexed)
		{
			array[indexed.Index] = indexed.Value;
			return array;
		}
	}
}