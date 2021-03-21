using System;
using System.Collections.Generic;
using System.Linq;

namespace Responsible.Utilities
{
	/// <summary>
	/// Safe, but not very efficient way of iterating through a list while it's modified.
	/// </summary>
	public class SafeIterationList<T>
	{
		private readonly List<Entry> entries = new List<Entry>();
		private bool modified;

		/// <summary>
		/// Adds item and returns a removal handle
		/// </summary>
		public IDisposable Add(T item)
		{
			this.modified = true;
			var entry = new Entry(item);
			this.entries.Add(entry);
			return Disposable.Create(() =>
			{
				this.modified = true;
				this.entries.Remove(entry);
			});
		}

		/// <summary>
		/// Enumerates through items once, allowing removals and additions.
		/// </summary>
		/// <returns>true, if the collection was modified, else false.</returns>
		public bool ForEach(Action<T> action)
		{
			foreach (var entry in this.entries)
			{
				entry.Enumerated = false;
			}

			bool EnumerateOnce()
			{
				this.modified = false;

				foreach (var entry in this.entries.Where(e => !e.Enumerated))
				{
					action(entry.Item);
					entry.Enumerated = true;

					if (this.modified)
					{
						return true;
					}
				}

				return false;
			}

			var wasModified = false;
			while (EnumerateOnce())
			{
				wasModified = true;
			}

			return wasModified;
		}

		private class Entry
		{
			public readonly T Item;
			public bool Enumerated { get; set; }

			public Entry(T item)
				=> this.Item = item;
		}
	}
}
