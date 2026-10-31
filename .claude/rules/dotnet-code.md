---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "Directory.Build.props"
  - "Directory.Packages.props"
---

# .NET code standards

- Follow `.editorconfig`. Code must pass `dotnet format --verify-no-changes`.
- `Nullable` enabled, `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`. Fix warnings, don't suppress them without a comment explaining why.
- Package versions only in `Directory.Packages.props` (Central Package Management).
- File-scoped namespaces. `_camelCase` private fields. Async methods end in `Async`.
- One top-level type per file, named after the type. Private nested types are fine. No unused usings. Both are enforced (architecture test, IDE0005).
- Async all the way. Accept and pass `CancellationToken`. Never `.Result` / `.Wait()` / `async void` (except event handlers).
- Records for DTOs and value objects.
- Options pattern for configuration (`IOptions<T>`), validated on start. No magic strings.
- `TimeProvider` for time. Store UTC `DateTimeOffset`. Never `DateTime.Now`.
- Small methods, early returns, clear names over comments.
