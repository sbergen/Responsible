using System;
using System.Linq;
using Responsible.Editor.Utilities;
using UnityEngine.UIElements;

namespace Responsible.Editor
{
	/// <summary>
	/// Very crude string class to help work around a Unity bug causing Labels to be truncated.
	/// See https://forum.unity.com/threads/generated-text-will-be-truncated-because-it-exceeds-49152-vertices.1126127/
	/// (I could not find the actual bug report referenced in that thread.)
	/// Splits labels that are too many lines long into multiple labels.
	/// Being a bit over-cautious here just in case...
	/// </summary>
	internal class AggregateLabel : VisualElement
	{
		private string[] subTexts = Array.Empty<string>();

		// ReSharper disable once InconsistentNaming, using Unity naming convention
		public string text
		{
			set
			{
				this.Clear();
				this.subTexts = value.SubstringsOfMaxLines(20).ToArray();
				foreach (var subText in this.subTexts)
				{
					this.Add(new Label(subText));
				}
			}
		}
	}
}
