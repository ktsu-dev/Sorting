// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Sorting;

using System.Globalization;

/// <summary>
/// Comparer that performs a natural comparison between strings, correctly comparing embedded numbers.
/// </summary>
/// <remarks>
/// A run of Unicode decimal digits (category <c>Nd</c>) is a number, whatever script it is written
/// in and whether or not it lies outside the Basic Multilingual Plane, and is compared by the value
/// it spells rather than by its code points. A number compared with text orders exactly as the
/// equivalent ASCII digits would, so every digit script sorts in the same place relative to text.
/// </remarks>
public partial class NaturalStringComparer : IComparer<string?>
{
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

		// Walk both strings a chunk at a time, in place, so a comparison allocates nothing. A sort
		// makes O(n log n) of them, and building each string's chunks first cost more than the
		// comparing did.
		int xIndex = 0;
		int yIndex = 0;
		while (xIndex < x.Length && yIndex < y.Length)
		{
			bool xIsNumeric = IsDigitAt(x, xIndex);
			bool yIsNumeric = IsDigitAt(y, yIndex);
			int xEnd = ChunkEnd(x, xIndex, xIsNumeric);
			int yEnd = ChunkEnd(y, yIndex, yIsNumeric);

			int comparison;
			if (xIsNumeric && yIsNumeric)
			{
				// If both chunks are numeric, compare them as numbers
				comparison = CompareNumericChunks(x, xIndex, xEnd, y, yIndex, yEnd);
			}
			else if (xIsNumeric || yIsNumeric)
			{
				// A number is compared with text as its ASCII digits would be, and a text chunk never
				// starts with a digit, so the first character decides. Comparing a non-ASCII digit's
				// own code point here would sort it after letters while its ASCII equal sorts before
				// them, which makes the order intransitive.
				comparison = xIsNumeric
					? AsciiDigitAt(x, xIndex).CompareTo(y[yIndex])
					: x[xIndex].CompareTo(AsciiDigitAt(y, yIndex));
			}
			else
			{
				// Otherwise, compare them as strings
				comparison = CompareTextChunks(x, xIndex, xEnd, y, yIndex, yEnd);
			}

			if (comparison != 0)
			{
				return comparison;
			}

			xIndex = xEnd;
			yIndex = yEnd;
		}

		// If we've exhausted one sequence but not the other, the shorter one comes first
		return xIndex < x.Length ? 1 : yIndex < y.Length ? -1 : 0;
	}

	/// <summary>
	/// Reports whether the code point at <paramref name="index"/> is a Unicode decimal digit
	/// (category <c>Nd</c>), reading a surrogate pair as the one code point it encodes.
	/// </summary>
	/// <param name="value">The string to read.</param>
	/// <param name="index">The index of the code point.</param>
	/// <returns>True if the code point is a decimal digit.</returns>
	private static bool IsDigitAt(string value, int index) =>
		CharUnicodeInfo.GetUnicodeCategory(value, index) == UnicodeCategory.DecimalDigitNumber;

	/// <summary>
	/// Returns how many UTF-16 code units the code point at <paramref name="index"/> takes.
	/// </summary>
	/// <param name="value">The string to read.</param>
	/// <param name="index">The index of the code point.</param>
	/// <returns>Two for a surrogate pair, otherwise one.</returns>
	private static int WidthAt(string value, int index) => char.IsSurrogatePair(value, index) ? 2 : 1;

	/// <summary>
	/// Returns the ASCII digit with the same value as the decimal digit at <paramref name="index"/>.
	/// </summary>
	/// <param name="value">The string to read.</param>
	/// <param name="index">The index of a decimal digit.</param>
	/// <returns>The digit, rewritten in ASCII.</returns>
	private static char AsciiDigitAt(string value, int index) =>
		(char)('0' + CharUnicodeInfo.GetDecimalDigitValue(value, index));

	/// <summary>
	/// Finds where the run of digits, or of other text, that starts at <paramref name="start"/> ends,
	/// walking by code point so that a digit encoded as a surrogate pair still counts as a digit.
	/// </summary>
	/// <param name="value">The string to scan.</param>
	/// <param name="start">The index the chunk starts at.</param>
	/// <param name="isNumeric">Whether the chunk is a run of decimal digits.</param>
	/// <returns>The index just past the chunk.</returns>
	private static int ChunkEnd(string value, int start, bool isNumeric)
	{
		int index = start;
		while (index < value.Length && IsDigitAt(value, index) == isNumeric)
		{
			index += WidthAt(value, index);
		}

		return index;
	}

	/// <summary>
	/// Compares two runs of text ordinally, the shorter first when one is a prefix of the other.
	/// </summary>
	/// <param name="x">The first string.</param>
	/// <param name="xStart">Where its chunk starts.</param>
	/// <param name="xEnd">Where its chunk ends.</param>
	/// <param name="y">The second string.</param>
	/// <param name="yStart">Where its chunk starts.</param>
	/// <param name="yEnd">Where its chunk ends.</param>
	/// <returns>A negative number, zero, or a positive number, as for <see cref="Compare"/>.</returns>
	private static int CompareTextChunks(string x, int xStart, int xEnd, string y, int yStart, int yEnd)
	{
		int xLength = xEnd - xStart;
		int yLength = yEnd - yStart;
		int comparison = string.CompareOrdinal(x, xStart, y, yStart, Math.Min(xLength, yLength));
		return comparison != 0 ? comparison : xLength.CompareTo(yLength);
	}

	/// <summary>
	/// Compares two runs of decimal digits by the numeric value they spell, rather than by their
	/// code points.
	/// </summary>
	/// <remarks>
	/// Each digit is read by its value, which is what keeps non-ASCII digit scripts ordering by
	/// magnitude: <c>'٥'</c> (Arabic-Indic five) is numerically less than <c>'9'</c>, even though its
	/// code point is far greater.
	/// </remarks>
	/// <param name="x">The first string.</param>
	/// <param name="xStart">Where its digits start.</param>
	/// <param name="xEnd">Where its digits end.</param>
	/// <param name="y">The second string.</param>
	/// <param name="yStart">Where its digits start.</param>
	/// <param name="yEnd">Where its digits end.</param>
	/// <returns>A negative number, zero, or a positive number, as for <see cref="Compare"/>.</returns>
	private static int CompareNumericChunks(string x, int xStart, int xEnd, string y, int yStart, int yEnd)
	{
		int xIndex = SkipLeadingZeros(x, xStart, xEnd);
		int yIndex = SkipLeadingZeros(y, yStart, yEnd);

		// With leading zeros gone, the chunk spelling more digits is the larger number
		int lengthComparison = CountDigits(x, xIndex, xEnd).CompareTo(CountDigits(y, yIndex, yEnd));
		if (lengthComparison != 0)
		{
			return lengthComparison;
		}

		// Same digit count, so the first differing digit decides
		while (xIndex < xEnd)
		{
			int digitComparison = AsciiDigitAt(x, xIndex).CompareTo(AsciiDigitAt(y, yIndex));
			if (digitComparison != 0)
			{
				return digitComparison;
			}

			xIndex += WidthAt(x, xIndex);
			yIndex += WidthAt(y, yIndex);
		}

		return 0;
	}

	/// <summary>
	/// Counts the digits between <paramref name="start"/> and <paramref name="end"/>, a surrogate pair
	/// counting as one.
	/// </summary>
	/// <param name="value">The string to read.</param>
	/// <param name="start">Where the digits start.</param>
	/// <param name="end">Where the digits end.</param>
	/// <returns>The number of digits.</returns>
	private static int CountDigits(string value, int start, int end)
	{
		int count = 0;
		for (int index = start; index < end; index += WidthAt(value, index))
		{
			count++;
		}

		return count;
	}

	/// <summary>
	/// Returns the index of the first digit in the run that is not a zero, or of its last digit when
	/// the run is all zeros, so a run of zeros compares as a single zero digit.
	/// </summary>
	/// <param name="value">The string to read.</param>
	/// <param name="start">Where the digits start.</param>
	/// <param name="end">Where the digits end.</param>
	/// <returns>The index at which the run's significant digits begin.</returns>
	private static int SkipLeadingZeros(string value, int start, int end)
	{
		int index = start;
		while (CharUnicodeInfo.GetDecimalDigitValue(value, index) == 0)
		{
			int next = index + WidthAt(value, index);
			if (next >= end)
			{
				break;
			}

			index = next;
		}

		return index;
	}
}
