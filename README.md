# ktsu.Sorting

A .NET library that provides advanced sorting utilities for specialized sorting scenarios.

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

## Features

### NaturalStringComparer
- Correctly sorts strings containing numbers in a human-friendly order
- Handles mixed alphanumeric strings
- Properly compares strings with leading zeros
- Fully compatible with .NET's `IComparer<string>` interface
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
