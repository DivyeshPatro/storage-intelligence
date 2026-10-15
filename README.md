# Storage Intelligence

A local-first, read-only Windows storage analyzer (C# / .NET 10, WPF, SQLite). It explains where disk space
goes, with exact, explainable accounting and honest coverage reporting.

## Status: milestone M0b — Windows scanner validation (pending review)

This repository contains the platform-independent accounting core, a read-only Windows scanner, and the validation
tooling used to prove it (independent reference scanner, guarded fixture generator, allocation harness). There is no
UI and no cleanup functionality.

| Milestone | Scope | Status |
|---|---|---|
| M0a | Accounting core: streaming aggregation, hard-link reconciliation, coverage, uncertainty; property-based tests against a brute-force oracle | Provisionally accepted |
| **M0b** | Windows scanner, reference scanner, fixture generator, CI volume probe, allocation validation | Pending review — [results](docs/m0b-validation-results.md), [CI capabilities](docs/ci-capabilities.md) |
| M0c | Measurements (CI, constrained VM); physical HDD validation pending hardware | Not started |
| M1+ | Minimal WPF shell for R1 | Blocked until accounting tests pass on Windows fixtures |

## Layout

```
src/StorageIntelligence.Accounting               accounting core (no Windows APIs)
src/StorageIntelligence.Scanning.Windows         read-only scanner (metadata only, never follows reparse points)
tools/StorageIntelligence.Validation             si-validate: reference scanner, fixture generator, allocation harness
tools/ci                                         CI scripts (disposable VHDX volumes, annotations)
tools/mutation-check.sh                          deliberate-bug check (rebuilds from clean afterwards)
tests/StorageIntelligence.Accounting.Tests       scenario tests, CsCheck property tests, brute-force oracle
tests/StorageIntelligence.Scanning.Windows.Tests Windows integration tests (skipped elsewhere)
docs/accounting-semantics.md             what every number means (read this first)
docs/m0b-validation-plan.md              validation plan
docs/m0b-validation-results.md           validation evidence and resulting measurement policy
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
