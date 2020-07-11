using System;
using System.Text;
using JetBrains.Annotations;
using Responsible.Context;

namespace Responsible.Utilities
{
	public abstract class IndentedStringBuilder<T>
		where T : IndentedStringBuilder<T>
	{
		private const int IndentChars = 2;

		private readonly StringBuilder stringBuilder = new StringBuilder();
		private int indentAmount;

		private T Self => (T)this;

		// Trim out last newline when converting to final string
		public override string ToString() => this.stringBuilder.ToString(0, this.stringBuilder.Length - 1);

		protected T Add(string content)
		{
			this.stringBuilder.Append(' ', this.indentAmount);
			this.stringBuilder.AppendLine(content);
			return this.Self;
		}

		protected T AddIndented(string content, [CanBeNull] Action<T> contextAdder)
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
	}
}