# Storage Intelligence

A local-first, read-only Windows storage analyzer (C# / .NET 10, WPF, SQLite). It explains where disk space
goes, with exact, explainable accounting and honest coverage reporting.

## Status: milestone M0a — accounting core

This repository currently contains **only** the platform-independent accounting core and its tests.
There is no UI, no Windows filesystem access and no cleanup functionality.

| Milestone | Scope | Status |
|---|---|---|
| **M0a** | Accounting core: streaming aggregation, hard-link reconciliation, coverage, uncertainty; property-based tests against a brute-force oracle | In review |
| M0b | Windows reader, reference scanner, fixture generator, Windows CI volume feasibility probe | Not started — [plan](docs/m0b-validation-plan.md) |
| M0c | Measurements (CI, constrained VM); physical HDD validation pending hardware | Not started |
| M1+ | Minimal WPF shell for R1 | Blocked until accounting tests pass on Windows fixtures |

## Layout

```
src/StorageIntelligence.Accounting        accounting core (no Windows APIs)
tests/StorageIntelligence.Accounting.Tests scenario tests, CsCheck property tests, brute-force oracle
docs/accounting-semantics.md             what every number means (read this first)
docs/m0b-validation-plan.md              how correctness will be validated on Windows
```

## Build and test

```
dotnet test
SI_PBT_ITER=1000000 dotnet test   # extended property-based run
```

## Principles

- Read-only. The R1 product contains no file-deletion code.
- Directory totals count **distinct files**; sibling totals can overlap (see the semantics document).
- Every measurement that has not been validated on a real filesystem is reported as **uncertain**.
