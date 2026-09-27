// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Sorting;

using System.Globalization;
using System.Text;

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

		List<Chunk> xChunks = SplitIntoChunks(x);
		List<Chunk> yChunks = SplitIntoChunks(y);

		int i = 0, j = 0;
		while (i < xChunks.Count && j < yChunks.Count)
		{
			Chunk xChunk = xChunks[i++];
			Chunk yChunk = yChunks[j++];

			// If both chunks are numeric, compare them as numbers
			if (xChunk.IsNumeric && yChunk.IsNumeric)
			{
				int numComparison = CompareNumericChunks(xChunk.Value, yChunk.Value);
				if (numComparison != 0)
				{
					return numComparison;
				}
			}
			else // Otherwise, compare them as strings
			{
				// A numeric chunk holds ASCII digits and a text chunk never starts with a digit, so a
				// number against text is decided by the first character, the same way for every
				// digit script. Comparing a non-ASCII digit's own code point here would sort it after
				// letters while its ASCII equal sorts before them, which makes the order intransitive.
				int stringComparison = string.Compare(xChunk.Value, yChunk.Value, StringComparison.Ordinal);
				if (stringComparison != 0)
				{
					return stringComparison;
				}
			}
		}

		// If we've exhausted one sequence but not the other, the shorter one comes first
		return xChunks.Count.CompareTo(yChunks.Count);
	}

	/// <summary>
	/// Splits a string into alternating runs of decimal digits and of other text, walking it by code
	/// point so that a digit encoded as a surrogate pair still counts as a digit.
	/// </summary>
	/// <remarks>
	/// A numeric chunk's value is rewritten in ASCII digits, so that numbers from every script compare
	/// alike both with each other and with text. A text chunk keeps its original characters.
	/// </remarks>
	/// <param name="value">The string to split.</param>
	/// <returns>The chunks of <paramref name="value"/>, in order.</returns>
	private static List<Chunk> SplitIntoChunks(string value)
	{
		List<Chunk> chunks = [];
		StringBuilder digits = new();
		int textStart = -1;
		int index = 0;
		while (index < value.Length)
		{
			int width = char.IsSurrogatePair(value, index) ? 2 : 1;
			if (CharUnicodeInfo.GetUnicodeCategory(value, index) == UnicodeCategory.DecimalDigitNumber)
			{
				if (textStart >= 0)
				{
					chunks.Add(new Chunk(false, value[textStart..index]));
					textStart = -1;
				}

				digits.Append((char)('0' + CharUnicodeInfo.GetDecimalDigitValue(value, index)));
			}
			else
			{
				if (digits.Length > 0)
				{
					chunks.Add(new Chunk(true, digits.ToString()));
					digits.Clear();
				}

				if (textStart < 0)
				{
					textStart = index;
				}
			}

			index += width;
		}

		if (digits.Length > 0)
		{
			chunks.Add(new Chunk(true, digits.ToString()));
		}
		else if (textStart >= 0)
		{
			chunks.Add(new Chunk(false, value[textStart..]));
		}

		return chunks;
	}

	/// <summary>
	/// Compares two chunks of decimal digits by the numeric value they spell, rather than by their
	/// code points.
	/// </summary>
	/// <remarks>
	/// Both chunks come from <see cref="SplitIntoChunks"/>, which has already rewritten every Unicode
	/// decimal digit (category <c>Nd</c>) as the ASCII digit of the same value. That is what keeps
	/// non-ASCII digit scripts ordering by magnitude: <c>'٥'</c> (Arabic-Indic five) is numerically
	/// less than <c>'9'</c>, even though its code point is far greater.
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
			int digitComparison = xChunk[xStart + offset].CompareTo(yChunk[yStart + offset]);
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
		while (index < chunk.Length - 1 && chunk[index] == '0')
		{
			index++;
		}

		return index;
	}

	/// <summary>
	/// A run of decimal digits, held as ASCII digits, or a run of other text.
	/// </summary>
	/// <param name="IsNumeric">Whether the chunk is a run of decimal digits.</param>
	/// <param name="Value">The chunk's ASCII digits when numeric, otherwise its original text.</param>
	private readonly record struct Chunk(bool IsNumeric, string Value);
}
