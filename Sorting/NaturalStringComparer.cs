// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Sorting;

using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Comparer that performs a natural comparison between strings, correctly comparing embedded numbers.
/// </summary>
/// <remarks>
/// A run of Unicode decimal digits (category <c>Nd</c>) is a number, whatever script it is written
/// in, and is compared by the value it spells rather than by its code points.
/// </remarks>
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
			if (char.IsDigit(xMatch.Value[0]) && char.IsDigit(yMatch.Value[0]))
			{
				int numComparison = CompareNumericChunks(xMatch.Value, yMatch.Value);
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

	/// <summary>
	/// Compares two chunks of decimal digits by the numeric value they spell, rather than by their
	/// code points.
	/// </summary>
	/// <remarks>
	/// Both chunks come from the <c>\d+</c> alternative of the chunk regex, so every character is a
	/// Unicode decimal digit (category <c>Nd</c>) and has a decimal value of 0-9. Comparing those
	/// values, rather than the raw UTF-16 code points, is what keeps non-ASCII digit scripts
	/// ordering by magnitude: <c>'٥'</c> (Arabic-Indic five) is numerically less than <c>'9'</c>,
	/// even though its code point is far greater.
	/// </remarks>
	/// <param name="xChunk">First digit chunk to compare.</param>
	/// <param name="yChunk">Second digit chunk to compare.</param>
	/// <returns>A negative number, zero, or a positive number, as for <see cref="Compare"/>.</returns>
	private static int CompareNumericChunks(string xChunk, string yChunk)
	{
		int xStart = SkipLeadingZeros(xChunk);
		int yStart = SkipLeadingZeros(yChunk);

		// With leading zeros gone, the chunk spelling more digits is the larger number
		int xDigits = xChunk.Length - xStart;
		int yDigits = yChunk.Length - yStart;
		int lengthComparison = xDigits.CompareTo(yDigits);
		if (lengthComparison != 0)
		{
			return lengthComparison;
		}

		// Same digit count, so the first differing digit decides
		for (int offset = 0; offset < xDigits; offset++)
		{
			int digitComparison = DigitValue(xChunk[xStart + offset]).CompareTo(DigitValue(yChunk[yStart + offset]));
			if (digitComparison != 0)
			{
				return digitComparison;
			}
		}

		return 0;
	}

	/// <summary>
	/// Returns the index of the first digit in <paramref name="chunk"/> that is not a zero, or
	/// <c>chunk.Length - 1</c> when the chunk is all zeros, so a chunk of zeros compares as a
	/// single zero digit.
	/// </summary>
	/// <param name="chunk">The digit chunk to scan.</param>
	/// <returns>The index at which the chunk's significant digits begin.</returns>
	private static int SkipLeadingZeros(string chunk)
	{
		int index = 0;
		while (index < chunk.Length - 1 && DigitValue(chunk[index]) == 0)
		{
			index++;
		}

		return index;
	}

	/// <summary>
	/// Returns the decimal value of a Unicode decimal digit, so that digits from any script compare
	/// by magnitude.
	/// </summary>
	/// <param name="digit">The digit character, which the chunk regex guarantees is category <c>Nd</c>.</param>
	/// <returns>The digit's value of 0-9.</returns>
	private static int DigitValue(char digit) => CharUnicodeInfo.GetDecimalDigitValue(digit);
}
