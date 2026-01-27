# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ktsu.Sorting is a .NET library providing advanced sorting utilities for specialized sorting scenarios. The primary component is `NaturalStringComparer`, which implements `IComparer<string?>` to sort strings containing embedded numbers in a human-friendly order (e.g., "file2" before "file10").

## Build Commands

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run a specific test
dotnet test --filter "FullyQualifiedName~NaturalStringComparerTests.Compare_NullValues_HandledCorrectly"

# Build in Release mode
dotnet build -c Release
```

## Architecture

- **Sorting/** - Main library project (targets multiple frameworks via ktsu.Sdk)
- **Sorting.Test/** - MSTest-based test project (targets net9.0)

The solution uses `ktsu.Sdk` which provides standardized build configuration. The test project uses MSTest.Sdk with Microsoft.Testing.Platform runner.

## Code Conventions

- Namespace: `ktsu.Sorting` (main), `ktsu.Sorting.Tests` (tests)
- Copyright header required on all source files
- Code follows standard C# conventions with XML documentation on public APIs
