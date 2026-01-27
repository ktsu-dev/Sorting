// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

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
		Assert.AreEqual(0, _comparer.Compare("file005", "file5")); // Numerically equal
		Assert.IsLessThan(0, _comparer.Compare("file005", "file06")); // 5 < 6 numerically
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
}
