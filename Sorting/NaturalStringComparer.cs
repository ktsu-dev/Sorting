// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Sorting;

using System.Text.RegularExpressions;

/// <summary>
/// Comparer that performs a natural comparison between strings, correctly comparing embedded numbers.
/// </summary>
public partial class NaturalStringComparer : IComparer<string?>
{
	/// <summary>
	/// Regular expression to match alphanumeric chunks in a string.
	/// </summary>
	[GeneratedRegex(@"(\d+)|(\D+)")]
	private static partial Regex CreateNaturalChunkRegex();

	/// <summary>
	/// Compares two strings using natural sorting, where embedded numbers are compared as numeric values.
	/// </summary>
	/// <param name="x">First string to compare.</param>
	/// <param name="y">Second string to compare.</param>
	/// <returns>
	/// Less than zero if x is less than y.
	/// Zero if x equals y.
	/// Greater than zero if x is greater than y.
	/// </returns>
	public int Compare(string? x, string? y)
	{
		if (x == null && y == null)
		{
			return 0;
		}

		if (x == null)
		{
			return -1;
		}

		if (y == null)
		{
			return 1;
		}

		if (x == y)
		{
			return 0;
		}

		var regex = CreateNaturalChunkRegex();
		var xMatches = regex.Matches(x).Cast<Match>().ToArray();
		var yMatches = regex.Matches(y).Cast<Match>().ToArray();

		int i = 0, j = 0;
		while (i < xMatches.Length && j < yMatches.Length)
		{
			var xMatch = xMatches[i++];
			var yMatch = yMatches[j++];

			// If both chunks are numeric, compare them as numbers
			if (int.TryParse(xMatch.Value, out var xNum) && int.TryParse(yMatch.Value, out var yNum))
			{
				var numComparison = xNum.CompareTo(yNum);
				if (numComparison != 0)
				{
					return numComparison;
				}
			}
			else // Otherwise, compare them as strings
			{
				var stringComparison = string.Compare(xMatch.Value, yMatch.Value, StringComparison.Ordinal);
				if (stringComparison != 0)
				{
					return stringComparison;
				}
			}
		}

		// If we've exhausted one sequence but not the other, the shorter one comes first
		return xMatches.Length.CompareTo(yMatches.Length);
	}
}
