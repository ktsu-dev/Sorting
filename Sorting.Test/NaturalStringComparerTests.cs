// Copyright (c) 2023-2026 ktsu-dev contributors

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace ktsu.Sorting.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class NaturalStringComparerTests
{
	private readonly NaturalStringComparer _comparer = new();

	[TestMethod]
	public void Compare_NullValues_HandledCorrectly()
	{
		// Both null should be equal
		Assert.AreEqual(0, _comparer.Compare(null, null));

		// null should be less than any string
		Assert.IsLessThan(0, _comparer.Compare(null, "any"));

		// Any string should be greater than null
		Assert.IsGreaterThan(0, _comparer.Compare("any", null));
	}

	[TestMethod]
	public void Compare_IdenticalStrings_ReturnsZero()
	{
		// Empty strings
		Assert.AreEqual(0, _comparer.Compare("", ""));

		// Simple strings
		Assert.AreEqual(0, _comparer.Compare("abc", "abc"));

		// Strings with numbers
		Assert.AreEqual(0, _comparer.Compare("abc123", "abc123"));
	}

	[TestMethod]
	public void Compare_SimpleStrings_SortsAlphabetically()
	{
		// Basic alphabetical comparisons
		Assert.IsLessThan(0, _comparer.Compare("a", "b"));
		Assert.IsGreaterThan(0, _comparer.Compare("b", "a"));

		// Case sensitivity (should be case-sensitive by default)
		Assert.IsGreaterThan(0, _comparer.Compare("a", "A")); // Uppercase comes before lowercase in ordinal comparison
	}

	[TestMethod]
	public void Compare_StringsWithNumbers_NaturalSortOrder()
	{
		// String with different numbers
		Assert.IsLessThan(0, _comparer.Compare("file2", "file10")); // Natural: 2 < 10

		// Compare with same prefix but different numbers
		Assert.IsLessThan(0, _comparer.Compare("image9.jpg", "image10.jpg"));

		// Multiple numbers in strings
		Assert.IsLessThan(0, _comparer.Compare("page-2-item-5", "page-2-item-10"));
		Assert.IsLessThan(0, _comparer.Compare("page-5-item-3", "page-10-item-1"));
	}

	[TestMethod]
	public void Compare_MixedAlphaNumeric_SortsNaturally()
	{
		// Test with a variety of real-world naming patterns
		string[] unsorted =
		[
			"z10.doc",
			"z1.doc",
			"z17.doc",
			"z2.doc",
			"z23.doc",
			"z3.doc"
		];

		string[] sorted =
		[
			"z1.doc",
			"z2.doc",
			"z3.doc",
			"z10.doc",
			"z17.doc",
			"z23.doc"
		];

		// Sort the array using our comparer
		Array.Sort(unsorted, _comparer);

		// Check that it matches our expected order
		CollectionAssert.AreEqual(sorted, unsorted);
	}

	[TestMethod]
	public void Compare_StringsWithLeadingZeros_HandledCorrectly()
	{
		// Numbers with leading zeros should be treated as numeric values
		AssertTiedOnlyByOrdinal("file005", "file5"); // Numerically equal, but distinct strings
		Assert.IsLessThan(0, _comparer.Compare("file005", "file06")); // 5 < 6 numerically
	}

	[TestMethod]
	public void Compare_NumericallyEqualStrings_AreNotEqual()
	{
		// Only ordinally equal strings may compare as zero, or sorted collections treat them as one key
		string[] values = ["file5", "file05", "file005", "file٥", "v1", "v01", "file5", "a"];
		foreach (string x in values)
		{
			foreach (string y in values)
			{
				Assert.AreEqual(string.Equals(x, y, StringComparison.Ordinal), _comparer.Compare(x, y) == 0, $"Compare(\"{x}\", \"{y}\")");
			}
		}
	}

	[TestMethod]
	public void Compare_NumericallyEqualStrings_StayBetweenTheirNeighbours()
	{
		// The tie-break only orders numerically equal strings among themselves
		foreach (string tied in new[] { "file5", "file05", "file005", "file٥" })
		{
			Assert.IsLessThan(0, _comparer.Compare("file4", tied));
			Assert.IsGreaterThan(0, _comparer.Compare("file6", tied));
		}
	}

	[TestMethod]
	public void SortedSet_KeepsStringsThatDifferOnlyInLeadingZeros()
	{
		SortedSet<string> set = new(_comparer) { "file5", "file05", "file005" };

		Assert.HasCount(3, set);
	}

	[TestMethod]
	public void SortedDictionary_AcceptsKeysThatDifferOnlyInLeadingZeros()
	{
		SortedDictionary<string, int> dictionary = new(_comparer)
		{
			["v1"] = 1,
		};
		dictionary.Add("v01", 2);

		Assert.HasCount(2, dictionary);
	}

	[TestMethod]
	public void Sort_IsTheSameForEveryInputOrder()
	{
		string[][] permutations =
		[
			["a", "b5", "b05"],
			["a", "b05", "b5"],
			["b5", "a", "b05"],
			["b5", "b05", "a"],
			["b05", "a", "b5"],
			["b05", "b5", "a"],
		];

		string[] expected = [.. permutations[0].OrderBy(s => s, _comparer)];
		foreach (string[] permutation in permutations)
		{
			Array.Sort(permutation, _comparer);
			CollectionAssert.AreEqual(expected, permutation);
		}
	}

	private void AssertTiedOnlyByOrdinal(string x, string y)
	{
		// Numerically equal but distinct strings compare by ordinal, never as equal
		int expected = Math.Sign(string.CompareOrdinal(x, y));
		Assert.AreNotEqual(0, expected);
		Assert.AreEqual(expected, Math.Sign(_comparer.Compare(x, y)), $"Compare(\"{x}\", \"{y}\")");
		Assert.AreEqual(-expected, Math.Sign(_comparer.Compare(y, x)), $"Compare(\"{y}\", \"{x}\")");
	}

	[TestMethod]
	public void Compare_NonAsciiDigits_ComparedByNumericValue()
	{
		// Arabic-Indic five is numerically less than ASCII nine, despite the far greater code point
		Assert.IsLessThan(0, _comparer.Compare("٥", "9"));
		Assert.IsGreaterThan(0, _comparer.Compare("9", "٥"));

		// The same holds within a single non-ASCII script
		Assert.IsLessThan(0, _comparer.Compare("٥", "٩")); // Arabic-Indic 5 < 9

		// Devanagari digits order by value too
		Assert.IsLessThan(0, _comparer.Compare("५", "३०")); // 5 < 30
	}

	[TestMethod]
	public void Compare_NonAsciiDigits_EqualValuesAreTiedOrdinally()
	{
		// Arabic-Indic five and ASCII five spell the same number
		AssertTiedOnlyByOrdinal("٥", "5");
		AssertTiedOnlyByOrdinal("file٥", "file5");
	}

	[TestMethod]
	public void Compare_NonAsciiLeadingZeros_NormalizedLikeAsciiZeros()
	{
		// An Arabic-Indic zero is a leading zero, so both chunks reduce to the single digit 0
		AssertTiedOnlyByOrdinal("٠0", "0");
		AssertTiedOnlyByOrdinal("٠٥", "5");

		// An all-zeros chunk compares as zero, whatever the script
		AssertTiedOnlyByOrdinal("٠٠", "0");
		Assert.IsLessThan(0, _comparer.Compare("٠٠", "1"));
	}

	[TestMethod]
	public void Compare_MixedScriptDigits_ComparedByNumericValue()
	{
		// A single chunk may mix scripts; it still spells one number
		Assert.IsLessThan(0, _comparer.Compare("file1٥", "file20")); // 15 < 20
		AssertTiedOnlyByOrdinal("file1٥", "file15");
	}

	[TestMethod]
	public void Compare_DifferentLengthStrings_ShorterComesFirst()
	{
		// When strings are identical up to the length of the shorter one
		Assert.IsLessThan(0, _comparer.Compare("abc", "abcdef"));
		Assert.IsGreaterThan(0, _comparer.Compare("abcdef", "abc"));

		// But not if the shorter string is lexicographically greater
		Assert.IsGreaterThan(0, _comparer.Compare("def", "abc123"));
	}

	[TestMethod]
	public void Compare_LargeNumbers_HandledCorrectly()
	{
		// Should handle numbers within integer range
		Assert.IsLessThan(0, _comparer.Compare("file1000000", "file9999999"));

		// Should handle numbers beyond Int32 range
		Assert.IsLessThan(0, _comparer.Compare("file9999999999", "file10000000000"));
	}

	[TestMethod]
	public void Compare_SortArray_CorrectOrder()
	{
		// A mix of alphanumeric strings to test sorting
		string[] files =
		[
			"file10.txt",
			"file1.txt",
			"file100.txt",
			"file12.txt",
			"file2.txt",
			"file20.txt",
			"fileb.txt",
			"filea.txt"
		];

		string[] expected =
		[
			"file1.txt",
			"file2.txt",
			"file10.txt",
			"file12.txt",
			"file20.txt",
			"file100.txt",
			"filea.txt",
			"fileb.txt"
		];

		Array.Sort(files, _comparer);

		CollectionAssert.AreEqual(expected, files);
	}

	[TestMethod]
	public void Compare_WithSpecialCharacters_SortsCorrectly()
	{
		// Test strings with special characters
		Assert.IsLessThan(0, _comparer.Compare("file-1", "file-2"));
		Assert.IsLessThan(0, _comparer.Compare("file_1", "file_2"));
		Assert.IsLessThan(0, _comparer.Compare("file 1", "file 2"));
	}

	[TestMethod]
	public void Compare_WithMixedNumbersAndText_SortsCorrectly()
	{
		string[] unsorted =
		[
			"10X",
			"1X",
			"9X",
			"2X"
		];

		string[] sorted =
		[
			"1X",
			"2X",
			"9X",
			"10X"
		];

		Array.Sort(unsorted, _comparer);

		CollectionAssert.AreEqual(sorted, unsorted);
	}

	[TestMethod]
	public void Compare_NonAsciiDigitAgainstText_OrdersLikeTheAsciiDigit()
	{
		// A digit chunk against a text chunk used to compare code points, so the Arabic-Indic five
		// sorted after letters while the ASCII five, which it equals, sorted before them.
		Assert.IsLessThan(0, _comparer.Compare("\u0665", "10"));
		Assert.IsLessThan(0, _comparer.Compare("10", "z"));
		Assert.IsLessThan(0, _comparer.Compare("\u0665", "z"));
		Assert.IsGreaterThan(0, _comparer.Compare("z", "\u0665"));
		Assert.IsGreaterThan(0, _comparer.Compare("\u0665", "-"));
		Assert.IsGreaterThan(0, _comparer.Compare("5", "-"));
		Assert.IsLessThan(0, _comparer.Compare("\u0665", "~"));
		Assert.IsLessThan(0, _comparer.Compare("5", "~"));
	}

	[TestMethod]
	public void Compare_SortOfMixedScriptDigitsAndText_DoesNotDependOnInputOrder()
	{
		string[] expected = ["\u0665", "10", "z"];

		string[] forward = ["\u0665", "10", "z"];
		string[] backward = ["z", "10", "\u0665"];
		Array.Sort(forward, _comparer);
		Array.Sort(backward, _comparer);

		CollectionAssert.AreEqual(expected, forward);
		CollectionAssert.AreEqual(expected, backward);
	}

	[TestMethod]
	public void Compare_IsTransitiveOverMixedScriptDigitsAndText()
	{
		string[] values =
		[
			"", "0", "5", "05", "10", "\u0665", "\u0661\u0660", "\U0001D7D7", "\U0001D7CF\U0001D7CE",
			"\u0969", "a", "z", "Z", " ", "-", "_", "~", "\u00e9", "\U0001F600",
			"a5", "a\u0665", "a10", "a-", "a z", "5a", "\u0665a", "10a", "-5", "~5",
		];

		foreach (string a in values)
		{
			foreach (string b in values)
			{
				Assert.AreEqual(Math.Sign(_comparer.Compare(a, b)), -Math.Sign(_comparer.Compare(b, a)), $"antisymmetry: '{a}' vs '{b}'");

				foreach (string c in values)
				{
					int ab = Math.Sign(_comparer.Compare(a, b));
					int bc = Math.Sign(_comparer.Compare(b, c));
					if (ab <= 0 && bc <= 0)
					{
						Assert.IsLessThanOrEqualTo(0, _comparer.Compare(a, c), $"transitivity: '{a}' <= '{b}' <= '{c}'");
					}
				}
			}
		}
	}

	[TestMethod]
	public void Compare_DigitsOutsideTheBasicMultilingualPlane_ComparedByNumericValue()
	{
		// Mathematical bold digits nine, and one followed by zero, are surrogate pairs in UTF-16.
		Assert.IsLessThan(0, _comparer.Compare("file\U0001D7D7", "file\U0001D7CF\U0001D7CE"));
		Assert.IsLessThan(0, _comparer.Compare("file\U0001D7D7", "file10"));
		Assert.IsGreaterThan(0, _comparer.Compare("file\U0001D7D7", "file8"));
		AssertTiedOnlyByOrdinal("file\U0001D7D7", "file9");
	}

	[TestMethod]
	public void Compare_AllocatesNothing()
	{
		// A sort makes O(n log n) comparisons, so anything a comparison allocates is paid that many
		// times over. Pairs cover text, ASCII and non-ASCII numbers, leading zeros, surrogate pairs,
		// and a number against text.
		(string X, string Y)[] pairs =
		[
			("file2.txt", "file10.txt"),
			("img007-final", "img7-draft"),
			("a\u0665b", "a5c"),
			("file\U0001D7D7", "file\U0001D7CF\U0001D7CE"),
			("5a", "a5"),
			("00000000000000000000000000000001", "1"),
			("same prefix, longer", "same prefix"),
		];

		// Warm up first, so that what is measured is the comparing rather than anything the runtime
		// does the first time a method is called.
		foreach ((string x, string y) in pairs)
		{
			_comparer.Compare(x, y);
		}

		long before = GC.GetAllocatedBytesForCurrentThread();
		for (int iteration = 0; iteration < 1000; iteration++)
		{
			foreach ((string x, string y) in pairs)
			{
				_comparer.Compare(x, y);
				_comparer.Compare(y, x);
			}
		}

		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

		Assert.AreEqual(0L, allocated, $"{allocated} bytes allocated over {pairs.Length * 2000} comparisons");
	}
}
