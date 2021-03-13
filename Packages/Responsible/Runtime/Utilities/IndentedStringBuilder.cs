using System;
using System.Text;
using JetBrains.Annotations;

namespace Responsible.Utilities
{
	/// <summary>
	/// Base class for indented string building. Not intended for public use.
	/// </summary>
	/// <typeparam name="T">Type of deriving class</typeparam>
	public abstract class IndentedStringBuilder<T>
		where T : IndentedStringBuilder<T>
	{
		private const int IndentChars = 2;

		private readonly StringBuilder stringBuilder = new StringBuilder();
		private int indentAmount;

		private T Self => (T)this;

		/// <summary>Converts the content into a string with the specified indentations.</summary>
		/// <returns>The string representation of the built content.</returns>
		// Trim out last newline when converting to final string
		public override string ToString() => this.stringBuilder.ToString(0, this.stringBuilder.Length - 1);

		private protected T Add(string content)
		{
			this.stringBuilder.Append(' ', this.indentAmount);
			this.stringBuilder.AppendLine(content);
			return this.Self;
		}

		private protected T AddIndented(string content, [CanBeNull] Action<T> contextAdder)
		{
			this.Add(content);
			if (contextAdder != null)
			{
				this.indentAmount += IndentChars;
				contextAdder(this.Self);
				this.indentAmount -= IndentChars;
			}

			return this.Self;
		}

		private protected T AddToPreviousLine(string addToEndOfPrevious)
		{
			this.stringBuilder.Insert(this.stringBuilder.Length - 1, addToEndOfPrevious);
			return this.Self;
		}

		private protected T AddEmptyLine()
		{
			this.stringBuilder.AppendLine(" "); // Add space so Unity doesn't strip it
			return this.Self;
		}
	}
}
