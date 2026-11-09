# ktsu.Sorting

A .NET library that provides advanced sorting utilities for specialized sorting scenarios.

[![License](https://img.shields.io/github/license/ktsu-dev/Sorting.svg?label=License&logo=nuget)](LICENSE.md)
[![NuGet Version](https://img.shields.io/nuget/v/ktsu.Sorting?label=Stable&logo=nuget)](https://nuget.org/packages/ktsu.Sorting)
[![NuGet Version](https://img.shields.io/nuget/vpre/ktsu.Sorting?label=Latest&logo=nuget)](https://nuget.org/packages/ktsu.Sorting)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.Sorting?label=Downloads&logo=nuget)](https://nuget.org/packages/ktsu.Sorting)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/ktsu-dev/Sorting?label=Commits&logo=github)](https://github.com/ktsu-dev/Sorting/commits/main)
[![GitHub contributors](https://img.shields.io/github/contributors/ktsu-dev/Sorting?label=Contributors&logo=github)](https://github.com/ktsu-dev/Sorting/graphs/contributors)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ktsu-dev/Sorting/dotnet.yml?branch=main&label=Build&logo=github)](https://github.com/ktsu-dev/Sorting/actions)

## Overview

ktsu.Sorting offers a collection of sorting utilities to address common sorting challenges that aren't easily handled by standard sorting methods. The library is designed to be modular and extensible, allowing you to use only the components you need.

## Components

### NaturalStringComparer

The first component in the ktsu.Sorting library is the NaturalStringComparer, which provides natural string comparison by treating embedded numeric parts as numbers rather than characters.

#### Problem

Standard string sorting sorts strings character by character, which results in unintuitive ordering for strings containing numbers:

```
"file1.txt"
"file10.txt"
"file2.txt"
```

#### Solution

NaturalStringComparer sorts strings the way a human would expect, recognizing and comparing embedded numbers as numeric values:

```
"file1.txt"
"file2.txt"
"file10.txt"
```

## Installation

### Package Manager
```
Install-Package ktsu.Sorting
```

### .NET CLI
```
dotnet add package ktsu.Sorting
```

## Usage

### Using NaturalStringComparer

```csharp
using ktsu.Sorting;

// Create a comparer instance
var comparer = new NaturalStringComparer();

// Compare individual strings
int result = comparer.Compare("file10.txt", "file2.txt"); // Returns > 0 (10 > 2)

// Sort an array using the natural comparer
string[] files = { "file10.txt", "file1.txt", "file100.txt", "file2.txt" };
Array.Sort(files, comparer);
// Result: "file1.txt", "file2.txt", "file10.txt", "file100.txt"

// Use with LINQ
var sortedFiles = files.OrderBy(file => file, new NaturalStringComparer());
```

### Ignoring case

By default text is compared ordinally, so every uppercase letter sorts before every lowercase one. Use `NaturalStringComparer.OrdinalIgnoreCase` (or `new NaturalStringComparer(ignoreCase: true)`) to list names the way Windows Explorer and Finder do:

```csharp
string[] files = { "file10.txt", "File2.txt", "file1.txt", "README.md", "build3.log", "Build20.log" };

Array.Sort(files, NaturalStringComparer.Ordinal);
// "Build20.log", "File2.txt", "README.md", "build3.log", "file1.txt", "file10.txt"

Array.Sort(files, NaturalStringComparer.OrdinalIgnoreCase);
// "build3.log", "Build20.log", "file1.txt", "File2.txt", "file10.txt", "README.md"
```

Strings that differ only in case are still never equal under `OrdinalIgnoreCase`. They are ordered next to each other, ordinally, so a `SortedSet<string>` keeps both `file1` and `File1`.

Case variants stay together even when a list also holds a variant that differs only in leading zeros or digit script. The two groupings can't both hold at once, so case wins: each spelling's case variants are adjacent, and the spellings sit side by side:

```csharp
string[] files = { "photo1.jpg", "photo01.jpg", "Photo1.jpg" };

Array.Sort(files, NaturalStringComparer.OrdinalIgnoreCase);
// "photo01.jpg", "Photo1.jpg", "photo1.jpg"
```

## Features

### NaturalStringComparer
- Correctly sorts strings containing numbers in a human-friendly order
- Handles mixed alphanumeric strings
- Properly compares strings with leading zeros
- Fully compatible with .NET's `IComparer<string>` interface
- Also implements the non-generic `System.Collections.IComparer`, for APIs that accept only that: `ArrayList.Sort`, WPF's `ListCollectionView.CustomSort`, WinForms' `ListView.ListViewItemSorter`. Arguments that aren't strings throw `ArgumentException`
- Can be used with `Array.Sort()`, LINQ's `OrderBy()`, and other sorting mechanisms

## Examples

### NaturalStringComparer Examples

The NaturalStringComparer correctly sorts strings like:

| Input           | Standard Sort    | Natural Sort     |
|-----------------|------------------|------------------|
| file1.txt       | file1.txt        | file1.txt        |
| file10.txt      | file10.txt       | file2.txt        |
| file2.txt       | file2.txt        | file10.txt       |
| -------------------------------    | -------------------------------    |
| z1.doc          | z1.doc           | z1.doc           |
| z10.doc         | z10.doc          | z2.doc           |
| z17.doc         | z17.doc          | z3.doc           |
| z2.doc          | z2.doc           | z10.doc          |
| z23.doc         | z23.doc          | z17.doc          |
| z3.doc          | z3.doc           | z23.doc          |

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.
