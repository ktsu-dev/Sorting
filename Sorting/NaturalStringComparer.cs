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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "<Pending>")]
	private static Regex CreateNaturalChunkRegex() => new(@"(\d+)|(\D+)");

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

		Regex regex = CreateNaturalChunkRegex();
		Match[] xMatches = [.. regex.Matches(x).Cast<Match>()];
		Match[] yMatches = [.. regex.Matches(y).Cast<Match>()];

		int i = 0, j = 0;
		while (i < xMatches.Length && j < yMatches.Length)
		{
			Match xMatch = xMatches[i++];
			Match yMatch = yMatches[j++];

			// If both chunks are numeric, compare them as numbers
			if (int.TryParse(xMatch.Value, out int xNum) && int.TryParse(yMatch.Value, out int yNum))
			{
				int numComparison = xNum.CompareTo(yNum);
				if (numComparison != 0)
				{
					return numComparison;
				}
			}
			else // Otherwise, compare them as strings
			{
				int stringComparison = string.Compare(xMatch.Value, yMatch.Value, StringComparison.Ordinal);
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
