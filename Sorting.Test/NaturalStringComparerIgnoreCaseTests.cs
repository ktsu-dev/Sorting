// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Sorting.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class NaturalStringComparerIgnoreCaseTests
{
	private readonly NaturalStringComparer _comparer = NaturalStringComparer.OrdinalIgnoreCase;

	[TestMethod]
	public void Sort_IgnoringCase_InterleavesNamesTheWayFileManagersDo()
	{
		string[] expected = ["build3.log", "Build20.log", "file1.txt", "File2.txt", "file10.txt", "README.md"];
		string[] files = ["file10.txt", "File2.txt", "file1.txt", "README.md", "build3.log", "Build20.log"];

		Array.Sort(files, _comparer);

		CollectionAssert.AreEqual(expected, files);
	}

	[TestMethod]
	public void Sort_ByDefault_StillPutsUppercaseFirst()
	{
		string[] expected = ["Build20.log", "File2.txt", "README.md", "build3.log", "file1.txt", "file10.txt"];
		string[] files = ["file10.txt", "File2.txt", "file1.txt", "README.md", "build3.log", "Build20.log"];
		string[] ordinal = [.. files];

		Array.Sort(files, new NaturalStringComparer());
		Array.Sort(ordinal, NaturalStringComparer.Ordinal);

		CollectionAssert.AreEqual(expected, files);
		CollectionAssert.AreEqual(expected, ordinal);
	}

	[TestMethod]
	public void Sort_IgnoringCase_KeepsCaseVariantsTogetherBesideALeadingZeroVariant()
	{
		string[] expected = ["photo01.jpg", "Photo1.jpg", "photo1.jpg"];
		string[] files = ["photo1.jpg", "photo01.jpg", "Photo1.jpg"];

		Array.Sort(files, _comparer);

		CollectionAssert.AreEqual(expected, files);
	}

	[TestMethod]
	public void Sort_IgnoringCase_GroupsEveryCaseVariantOfEachSpelling()
	{
		string[] expected = ["File05", "file05", "FILE5", "File5", "file5"];
		string[] files = ["file5", "File5", "file05", "File05", "FILE5"];

		Array.Sort(files, _comparer);

		CollectionAssert.AreEqual(expected, files);
	}

	[TestMethod]
	public void Sort_ByDefault_StillBreaksLeadingZeroTiesOrdinally()
	{
		string[] expected = ["FILE5", "File05", "File5", "file05", "file5"];
		string[] files = ["file5", "File5", "file05", "File05", "FILE5"];

		Array.Sort(files, NaturalStringComparer.Ordinal);

		CollectionAssert.AreEqual(expected, files);
	}

	[TestMethod]
	public void Compare_IgnoringCase_CaseVariantsBesideALeadingZeroVariantAreNotEqual()
	{
		Assert.AreNotEqual(0, _comparer.Compare("Photo1.jpg", "photo1.jpg"));
		Assert.AreNotEqual(0, _comparer.Compare("photo01.jpg", "Photo1.jpg"));
		Assert.IsTrue(_comparer.Compare("Photo1.jpg", "photo1.jpg") < 0);
	}

	[TestMethod]
	public void Constructor_IgnoreCase_MatchesTheStaticInstance()
	{
		NaturalStringComparer constructed = new(ignoreCase: true);
		string[] values = ["a", "A", "b", "B", "file2", "File10", "_", "5", "a5", "A_", ""];

		foreach (string x in values)
		{
			foreach (string y in values)
			{
				Assert.AreEqual(Math.Sign(_comparer.Compare(x, y)), Math.Sign(constructed.Compare(x, y)), $"Compare(\"{x}\", \"{y}\")");
			}
		}
	}

	[TestMethod]
	public void Compare_IgnoringCase_TiesStringsThatDifferOnlyInCaseOrdinally()
	{
		foreach ((string x, string y) in new[] { ("file1", "File1"), ("abc", "ABC"), ("aBc10", "AbC10") })
		{
			int expected = Math.Sign(string.CompareOrdinal(x, y));
			Assert.AreNotEqual(0, expected);
			Assert.AreEqual(expected, Math.Sign(_comparer.Compare(x, y)), $"Compare(\"{x}\", \"{y}\")");
			Assert.AreEqual(-expected, Math.Sign(_comparer.Compare(y, x)), $"Compare(\"{y}\", \"{x}\")");
		}

		Assert.AreEqual(0, _comparer.Compare("File1", "File1"));
	}

	[TestMethod]
	public void Compare_IgnoringCase_TiesStringsThatDifferInCaseAndLeadingZerosIgnoringCaseFirst()
	{
		// Ordinally "IMG7" < "img007", because case outranks the digits; ignoring case, "img007" < "IMG7".
		Assert.IsTrue(_comparer.Compare("img007", "IMG7") < 0);
		Assert.IsTrue(_comparer.Compare("IMG7", "img007") > 0);
	}

	[TestMethod]
	public void Compare_IgnoringCase_TiedStringsStayBetweenTheirNeighbours()
	{
		foreach (string tied in new[] { "file5", "File5", "FILE05" })
		{
			Assert.IsLessThan(0, _comparer.Compare("file4", tied));
			Assert.IsLessThan(0, _comparer.Compare("FILE4", tied));
			Assert.IsGreaterThan(0, _comparer.Compare("file6", tied));
			Assert.IsGreaterThan(0, _comparer.Compare("File6", tied));
		}
	}

	[TestMethod]
	public void SortedSet_IgnoringCase_KeepsStringsThatDifferOnlyInCase()
	{
		SortedSet<string> set = new(_comparer) { "file1", "File1", "FILE1" };

		Assert.HasCount(3, set);
	}

	[TestMethod]
	public void Compare_IgnoringCase_IsTransitiveOverMixedCaseDigitsAndText()
	{
		// Folding moves letters relative to the punctuation between 'Z' and 'a' ("_" sorts after "a"
		// once "a" reads as "A"), so the values mix both cases with that punctuation, digits in several
		// scripts, and numbers meeting text.
		string[] values =
		[
			"", "0", "5", "05", "10", "٥", "\U0001D7D7", "a", "A", "z", "Z", "_", "^", "[", "`", "{",
			" ", "-", "~", "é", "É", "ß", "ı", "I", "i", "İ", "a5", "A5", "a10",
			"A_", "a_", "a[", "A[", "a`", "5a", "5A", "5_", "10a", "a.b", "A.B", "a.5", "a z", "A Z",
			"file1", "File1", "file_1", "File_1", "fileA", "filea", "file10", "FILE10", "\U0001F600",
		];

		foreach (string a in values)
		{
			foreach (string b in values)
			{
				int ab = Math.Sign(_comparer.Compare(a, b));
				Assert.AreEqual(ab, -Math.Sign(_comparer.Compare(b, a)), $"antisymmetry: '{a}' vs '{b}'");
				Assert.AreEqual(string.Equals(a, b, StringComparison.Ordinal), ab == 0, $"only equal strings tie: '{a}' vs '{b}'");

				foreach (string c in values)
				{
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
	public void Compare_IgnoringCase_FoldsLettersOutsideTheBmp()
	{
		// Deseret small/capital long I, and Adlam small/capital alif: cased letters encoded as surrogate
		// pairs, which StringComparison.OrdinalIgnoreCase folds by code point.
		Assert.IsLessThan(0, string.Compare("\U00010428a", "\U00010400b", StringComparison.OrdinalIgnoreCase));
		Assert.IsLessThan(0, _comparer.Compare("\U00010428a", "\U00010400b"));
		Assert.IsGreaterThan(0, _comparer.Compare("\U00010400b", "\U00010428a"));
		Assert.IsLessThan(0, _comparer.Compare("\U0001E922a", "\U0001E900b"));
		Assert.IsLessThan(0, _comparer.Compare("file\U0001E9222", "File\U0001E90010"));
	}

	[TestMethod]
	public void Sort_IgnoringCase_KeepsSupplementaryNamesEqualIgnoringCaseTogether()
	{
		string[] names = ["\U0001E900zzz", "\U0001E922file", "\U0001E900file", "\U0001E923a", "\U0001E901b"];

		Array.Sort(names, _comparer);

		int lower = Array.IndexOf(names, "\U0001E922file");
		int upper = Array.IndexOf(names, "\U0001E900file");
		Assert.AreEqual(1, Math.Abs(lower - upper), string.Join(", ", names));
		Assert.AreNotEqual(0, _comparer.Compare("\U0001E922file", "\U0001E900file"));
		Assert.AreEqual(Math.Sign(string.CompareOrdinal("\U0001E922file", "\U0001E900file")), Math.Sign(_comparer.Compare("\U0001E922file", "\U0001E900file")));
	}

	[TestMethod]
	public void Compare_IgnoringCase_FoldsAsOrdinalIgnoreCaseDoes()
	{
		// U+E000..U+FFFF are left out: like Ordinal, the comparer orders them after a surrogate pair, by
		// code unit, where OrdinalIgnoreCase orders them before it, by code point.
		string[] values =
		[
			"a", "A", "\U00010428", "\U00010400", "\U0001E922", "\U0001E900", "\U000104D8", "\U000104B0",
			"\U0001F600", "\u00E9", "\u00C9", "_", "z", "\U00010428\U00010428", "\U00010400a",
		];

		foreach (string a in values)
		{
			foreach (string b in values)
			{
				int expected = Math.Sign(string.Compare(a, b, StringComparison.OrdinalIgnoreCase));
				if (expected == 0)
				{
					expected = Math.Sign(string.CompareOrdinal(a, b));
				}

				Assert.AreEqual(expected, Math.Sign(_comparer.Compare(a, b)), $"'{a}' vs '{b}'");
			}
		}
	}

	[TestMethod]
	public void Compare_IgnoringCase_IsTransitiveOverBmpAndSupplementaryLetters()
	{
		// A pair can straddle the end of the shorter text chunk ("a" + pair against "a" + digit), and
		// U+E000..U+FFFF sort after surrogates by code unit, so mix both in with digits.
		string[] values =
		[
			"", "a", "A", "_", "\uE000", "\uFF41", "\uFF21", "\U00010428", "\U00010400", "\U0001E922", "\U0001E900",
			"\U0001F600", "a\U00010428", "A\U00010400", "a\U0001E922", "a5", "A5", "a\U0001D7D7", "\U000104282",
			"\U0001040010", "\U00010428_", "\U00010400a", "\U0001E922file", "\U0001E900file", "\U0001E900zzz",
			"\uD801", "\uDC28", "a\uD801", "5\U00010428",
		];

		foreach (string a in values)
		{
			foreach (string b in values)
			{
				int ab = Math.Sign(_comparer.Compare(a, b));
				Assert.AreEqual(ab, -Math.Sign(_comparer.Compare(b, a)), $"antisymmetry: '{a}' vs '{b}'");
				Assert.AreEqual(string.Equals(a, b, StringComparison.Ordinal), ab == 0, $"only equal strings tie: '{a}' vs '{b}'");

				foreach (string c in values)
				{
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
	public void Compare_IgnoringCase_LeavesLongSUnfoldedAsOrdinalIgnoreCaseDoes()
	{
		// char.ToUpperInvariant('ſ') is 'S', but OrdinalIgnoreCase leaves U+017F as it is.
		Assert.IsTrue(_comparer.Compare("ſ", "t") > 0, "ſ sorts after t");
		Assert.IsTrue(_comparer.Compare("t", "ſ") < 0, "t sorts before ſ");

		foreach (string other in new[] { "s", "S", "t", "T" })
		{
			int expected = Math.Sign(string.Compare("ſ", other, StringComparison.OrdinalIgnoreCase));
			Assert.AreEqual(expected, Math.Sign(_comparer.Compare("ſ", other)), $"Compare(\"ſ\", \"{other}\")");
			Assert.AreEqual(-expected, Math.Sign(_comparer.Compare(other, "ſ")), $"Compare(\"{other}\", \"ſ\")");
		}

		string[] files = ["Tag1.txt", "ſtraße2.txt", "Rose1.txt"];
		string[] expectedOrder = [.. files];
		Array.Sort(files, _comparer);
		Array.Sort(expectedOrder, StringComparer.OrdinalIgnoreCase);
		CollectionAssert.AreEqual(expectedOrder, files);
	}

	[TestMethod]
	public void Compare_IgnoringCase_FoldsEveryBmpLetterAsOrdinalIgnoreCaseDoes()
	{
		string[] probes = ["a", "s", "S", "t", "T", "z", "ß", "é", "Ω", "я"];

		for (int code = 0; code <= char.MaxValue; code++)
		{
			char c = (char)code;
			if (char.IsSurrogate(c) || !char.IsLetter(c))
			{
				continue;
			}

			string value = c.ToString();
			foreach (string probe in probes)
			{
				int expected = Math.Sign(string.Compare(value, probe, StringComparison.OrdinalIgnoreCase));
				if (expected == 0)
				{
					continue;
				}

				Assert.AreEqual(expected, Math.Sign(_comparer.Compare(value, probe)), $"Compare(U+{code:X4}, \"{probe}\")");
			}
		}
	}

	[TestMethod]
	public void Compare_IgnoringCase_AllocatesNothing()
	{
		(string X, string Y)[] pairs =
		[
			("file2.txt", "File10.txt"),
			("IMG007-final", "img7-Draft"),
			("a٥B", "A5c"),
			("FILE\U0001D7D7", "file\U0001D7CF\U0001D7CE"),
			("5A", "a5"),
			("Same Prefix, Longer", "same prefix"),
			("abc", "ABC"),
			("\U00010428a", "\U00010400b"),
		];

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

	[TestMethod]
	public void Compare_DigitScriptsWithNoCaseDifference_OrdersAsOrdinalDoes()
	{
		// Fullwidth U+FF11 against Mathematical U+1D7CF: no case differs, so ignoring case must not
		// change which comes first. OrdinalIgnoreCase puts U+E000-U+FFFF before surrogate pairs.
		string fullwidth = "file\uFF11";
		string mathematical = "file\U0001D7CF";

		Assert.AreEqual(
			Math.Sign(NaturalStringComparer.Ordinal.Compare(fullwidth, mathematical)),
			Math.Sign(_comparer.Compare(fullwidth, mathematical)));
		Assert.AreEqual(
			Math.Sign(NaturalStringComparer.Ordinal.Compare(mathematical, fullwidth)),
			Math.Sign(_comparer.Compare(mathematical, fullwidth)));
	}
}
