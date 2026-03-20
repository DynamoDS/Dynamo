# Test Quality Guidelines & Anti-Patterns

## Test Quality Checklist

- ✅ **One behavior per test** - Each test validates exactly one scenario
- ✅ **Descriptive names** - Test name clearly describes the scenario
- ✅ **Arrange-Act-Assert** - Clear separation of test phases
- ✅ **Independent tests** - Tests don't depend on each other's state
- ✅ **Proper cleanup** - Resources are disposed, temp files removed
- ✅ **Meaningful assertions** - Assertions validate the important behavior
- ✅ **No hardcoded paths** - Use TestDirectory, SampleDirectory, TempFolder
- ✅ **Category tags** - Apply appropriate [Category] attributes

## Common Anti-Patterns to Avoid

### ❌ Multiple behaviors in one test
```csharp
// DON'T: Testing multiple unrelated things
[Test]
public void TestEverything()
{
    // Tests creation, execution, and saving all in one test
}
```

### ✅ Split into focused tests
```csharp
[Test] public void NodeCreatedWithCorrectProperties() { }
[Test] public void NodeExecutesAndProducesExpectedOutput() { }
[Test] public void WorkspaceSavesWithCorrectStructure() { }
```

### ❌ Ignored tests without explanation
```csharp
[Test]
[Ignore] // DON'T: No explanation why
public void SomeTest() { }
```

### ✅ Documented ignored tests
```csharp
[Test]
[Ignore("WIP - Node execution logic being refactored in DYN-1234")]
public void NewNodeExecutesCorrectly() { }
```

### ❌ Empty catch blocks
```csharp
try
{
    RiskyOperation();
}
catch
{
    // DON'T: Silent failure
}
```

### ✅ Proper error handling
```csharp
try
{
    RiskyOperation();
}
catch (ExpectedException ex)
{
    // Log the error for debugging
    Console.WriteLine($"Expected error: {ex.Message}");
    // Re-throw or handle appropriately
    throw;
}
```

### ❌ Weak assertions
```csharp
// DON'T: Weakened assertions
Assert.IsNotNull(result); // When you could be more specific

// DO: Specific assertions
Assert.AreEqual(expectedValue, result);
Assert.AreEqual(3, collection.Count());
```

### ❌ Tests with side effects
```csharp
// DON'T: Tests affecting global state
[Test]
public void TestThatChangesGlobalConfig()
{
    GlobalConfig.Setting = "test value"; // Affects other tests
    // ... test logic
    // No cleanup
}
```

### ✅ Isolated tests with cleanup
```csharp
[Test]
public void TestWithProperIsolation()
{
    // Arrange
    var originalValue = GlobalConfig.Setting;

    try
    {
        // Act
        GlobalConfig.Setting = "test value";
        // ... test logic
    }
    finally
    {
        // Cleanup
        GlobalConfig.Setting = originalValue;
    }
}
```

## File Organization Guidelines

### Test File Structure
```
test/
├── DynamoCoreTests/           # Core engine tests
│   ├── Graph/
│   ├── Nodes/
│   └── Utilities/
├── DynamoCoreWpfTests/        # UI component tests
├── Libraries/                 # Per-library tests
│   ├── CoreNodesTests/
│   └── PythonNodeModelsTests/
└── System/                    # Integration tests
```

### Test Categories
- `[Category("UnitTests")]` - Unit tests (including focused .dyn file tests)
- `[Category("RegressionTests")]` - Tests for specific bug fixes
- `[Category("Failure")]` - Tests that demonstrate known failures

**Note**: Dynamo uses a pragmatic approach where tests loading .dyn files can be categorized as unit tests when they focus on testing specific functionality rather than broad integration scenarios.

## Naming Conventions

### Preferred Naming Styles

1. **Descriptive approach** (when scenario is clear):
   ```csharp
   HomeWorkspaceCanSaveAsNewFile()
   CustomNodeFunctionalityWorksCorrectly()
   ```

2. **When/Then format** (for complex scenarios):
   ```csharp
   WhenNodeIsCreatedThenItHasCorrectProperties()
   WhenInvalidInputThenThrowsException()
   ```

### Naming Guidelines
- Use present tense for actions
- Be specific about the scenario being tested
- Include the expected outcome when it's not obvious
- Avoid generic names like `Test1()` or `TestMethod()`
