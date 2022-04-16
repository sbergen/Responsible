using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ResponsibleGherkin.Tests;

public class EnumValues<T> : IEnumerable<object[]>
	where T : struct, Enum
{
	private static readonly List<object[]> Values = Enum
		.GetValues(typeof(T))
		.Cast<object>()
		.Select(val => new[] { val })
		.ToList();

	public IEnumerator<object[]> GetEnumerator() => Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
