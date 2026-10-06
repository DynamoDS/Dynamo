# C# Patterns Reference

## Modern C# (use when TFM supports it — .NET 10 / C# 14)

- **Records** for DTOs and immutable data: `public record CustomerDto(string Id, string Name);`
- **Pattern matching** and switch expressions over long if-else chains.
- **File-scoped namespaces**: `namespace Dynamo.Graph.Workspaces;`
- **Raw string literals** for multi-line strings.
- **Collection expressions**: `List<int> items = [1, 2, 3];`
- **Primary constructors** for simple DI: `public sealed class Service(ILogger<Service> logger)`
- **Ranges and indices**: `span[^1]`, `array[1..3]`

---

## Type Design

| Goal | Pattern |
|---|---|
| DTO / immutable data | `record` |
| Small value type (< 16 bytes) | `readonly record struct` |
| Class not designed for inheritance | `sealed` (enables JIT devirtualization) |
| API boundary collection | `IReadOnlyList<T>`, `FrozenSet<T>` |
| Logic with no instance state | `static` pure method |

- Use `init`-only properties and `required` members for construction validation.
- Favor `with` expressions for modifications: `var updated = original with { Name = "new" };`

---

## Error Handling

- Guard early: `ArgumentNullException.ThrowIfNull(x)`, `string.IsNullOrWhiteSpace(x)`.
- Use precise exception types: `ArgumentException`, `InvalidOperationException` — never throw base `Exception`.
- No silent catches: don't swallow errors. Log and rethrow, or let them bubble.
- No blanket `catch (Exception)` without good reason.

---

## Async Programming

- All async methods end with `Async`.
- Always await calls — no fire-and-forget.
- Pass `CancellationToken` end-to-end. Call `ThrowIfCancellationRequested()` in loops.
- Use `ConfigureAwait(false)` in library code; omit in app entry/UI.
- Prefer `Task` over `ValueTask` unless benchmarks show otherwise.
- Stream large payloads: `GetAsync(..., ResponseHeadersRead)` then `ReadAsStreamAsync`.
- **Timeouts**: use a linked `CancellationTokenSource` + `CancelAfter` rather than `Task.WhenAny` — this actually cancels the work:
  ```csharp
  using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
  linked.CancelAfter(TimeSpan.FromSeconds(10));
  ```
- **Async dispose**: prefer `await using` for streams, HTTP responses, and other async-disposable resources.
- **Don't wrap needlessly**: if a method only returns another task with no extra logic, return the task directly — skip the `async`/`await` overhead.

---

## Performance

- Simple first; optimize hot paths only when measured.
- Use `Span<T>` / `Memory<T>` / pooling when benchmarks justify it.
- Avoid LINQ in tight loops — prefer `foreach` with early exit.
- Defer enumeration: return `IEnumerable<T>` instead of materializing to `List<T>` when downstream only iterates.
- Don't allocate in hot paths: prefer `string.Create`, `stackalloc`, or `ArrayPool<T>`.

---

## WPF / UI (DynamoCoreWpf)

- Follow MVVM: ViewModels hold state and commands; Views bind to them.
- UI thread access: use `Dispatcher.Invoke` / `DispatcherObject.CheckAccess()` — never touch UI elements from background threads.
- Implement `INotifyPropertyChanged` via `OnPropertyChanged(nameof(Property))`.
- Use `RelayCommand` / `DelegateCommand` patterns already present in the codebase — don't introduce new command frameworks.
- Avoid code-behind logic; keep Views as thin as possible.

---

## Examples

### Adding a new public method to DynamoCore

```csharp
/// <summary>
/// Returns the workspace name for the given path.
/// </summary>
/// <param name="filePath">Absolute path to the .dyn file.</param>
/// <returns>The workspace name, or null if the file does not exist.</returns>
public string? GetWorkspaceName(string filePath)
{
    ArgumentNullException.ThrowIfNull(filePath);
    // ...
}
```

Then add to `src/DynamoCore/PublicAPI.Unshipped.txt`:
```
Dynamo.Core.DynamoModel.GetWorkspaceName(string) -> string?
```

### Reviewing error handling in a PR

Before (bad):
```csharp
try { DoWork(); }
catch { } // swallows all errors
```

After (good):
```csharp
try { DoWork(); }
catch (InvalidOperationException ex)
{
    logger.LogWarning(ex, "DoWork failed for {Input}", input);
    throw;
}
```
