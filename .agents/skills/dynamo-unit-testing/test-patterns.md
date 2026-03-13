# Dynamo Test Patterns Reference

## Base Class Selection

### UnitTestBase
- **Use for**: Basic unit tests without Dynamo model
- **Provides**: TempFolder, TestDirectory, ExecutingDirectory, Setup/Teardown
- **Example**: Testing utility classes, data structures, algorithms

```csharp
public class MyUtilityTests : UnitTestBase
{
    [Test]
    public void WhenValidInputThenReturnsExpectedResult()
    {
        // Test standalone utility methods
    }
}
```

### DynamoModelTestBase
- **Use for**: Tests requiring DynamoModel (most Dynamo functionality)
- **Provides**: CurrentDynamoModel, preloader, pathResolver, library management
- **Example**: Testing nodes, workspaces, command execution

```csharp
public class NodeBehaviorTests : DynamoModelTestBase
{
    protected override void GetLibrariesToPreload(List<string> libraries)
    {
        libraries.Add("CoreNodeModels.dll");
        libraries.Add("Builtin.dll");
        base.GetLibrariesToPreload(libraries);
    }

    [Test]
    public void WhenNodeExecutesThenOutputIsValid()
    {
        // Test node behavior within DynamoModel
    }
}
```

### DSEvaluationUnitTestBase
- **Use for**: Tests involving DesignScript evaluation
- **Provides**: Engine evaluation context, mirror utilities
- **Example**: Testing mathematical operations, DS expression evaluation

## Common Testing Patterns

### 1. Node Testing Pattern

```csharp
[Test]
public void NodeCreatedWithExpectedProperties()
{
    // Arrange
    var nodeModel = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));

    // Act
    var createCommand = new DynamoModel.CreateNodeCommand(nodeModel, 0, 0, false, false);
    CurrentDynamoModel.ExecuteCommand(createCommand);

    // Assert
    Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
    Assert.IsNotNull(nodeModel.GUID);
}
```

### 2. Workspace Testing Pattern

```csharp
[Test]
public void WorkspaceSavesFileSuccessfully()
{
    // Arrange
    var newPath = GetNewFileNameOnTempPath("dyn");

    // Act
    CurrentDynamoModel.CurrentWorkspace.Save(newPath);

    // Assert
    Assert.IsTrue(File.Exists(newPath));
}
```

### 3. .dyn File Testing Pattern

```csharp
[Test]
[Category("UnitTests")] // Pragmatic: .dyn file tests can be unit tests when testing specific functionality
public void DynFileLoadsWithCorrectNodes()
{
    // Arrange
    var dynFilePath = Path.Combine(TestDirectory, @"core\nodesamples\TestGraph.dyn");

    // Act
    OpenModel(dynFilePath);
    RunCurrentModel();

    // Assert
    Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
    AssertPreviewValue("node-guid-here", expectedValue);
}
```

### 4. Custom Node Testing Pattern

```csharp
[Test]
public void CustomNodeCanSaveAsFile()
{
    // Arrange
    var nodeName = "MyCustomNode";
    var category = "Custom Nodes";

    // Act
    var customNodeWs = CurrentDynamoModel.CustomNodeManager.CreateCustomNode(nodeName, category, "");
    var newPath = GetNewFileNameOnTempPath("dyf");
    customNodeWs.Save(newPath);

    // Assert
    Assert.IsTrue(File.Exists(newPath));
    Assert.AreEqual(nodeName, customNodeWs.Name);
}
```

### 5. Command Testing Pattern

```csharp
[Test]
public void UndoCommandRevertsWorkspace()
{
    // Arrange
    var initialNodeCount = CurrentDynamoModel.CurrentWorkspace.Nodes.Count();
    var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));

    // Act
    CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, false, false));
    Assert.AreEqual(initialNodeCount + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

    CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo));

    // Assert
    Assert.AreEqual(initialNodeCount, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
}
```

## Setup and Teardown Templates

### Standard Setup
```csharp
[SetUp]
public override void Setup()
{
    base.Setup(); // Always call base.Setup() first

    // Test-specific setup
    // Initialize test data, configure settings
}

[TearDown]
public override void Cleanup()
{
    try
    {
        // Test-specific cleanup
        // Reset state, clear selections

        if (CurrentDynamoModel != null)
        {
            DynamoSelection.Instance.ClearSelection();
        }
    }
    finally
    {
        base.Cleanup(); // Always call base.Cleanup() last
    }
}
```

### Library Preloading

```csharp
protected override void GetLibrariesToPreload(List<string> libraries)
{
    libraries.Add("CoreNodeModels.dll");
    libraries.Add("Builtin.dll");
    libraries.Add("DSCoreNodes.dll");
    libraries.Add("FFITarget.dll"); // For testing specific functionality
    base.GetLibrariesToPreload(libraries);
}
```

## Common Assertion Patterns

### Value Assertions
```csharp
// Preview value testing
AssertPreviewValue("node-guid", expectedValue);

// Collection assertions
Assert.AreEqual(expectedCount, collection.Count());
Assert.IsTrue(collection.Any(item => item.Property == expectedValue));

// State assertions
Assert.IsTrue(workspace.CanUndo);
Assert.IsFalse(workspace.HasUnsavedChanges);
```

### Exception Testing
```csharp
[Test]
public void InvalidInputThrowsException()
{
    // Arrange
    var invalidInput = null;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => MyMethod(invalidInput));
}
```

### File and Path Testing
```csharp
// Use base class methods for temp files
var tempPath = GetNewFileNameOnTempPath("dyn");
var tempDir = Path.Combine(TempFolder, "subfolder");
Directory.CreateDirectory(tempDir);

// Reference test files correctly
var testFilePath = Path.Combine(TestDirectory, @"core\nodesamples\MyTestFile.dyn");
var samplePath = Path.Combine(SampleDirectory, "MySample.dyn");
```

## Test Infrastructure Integration

### MockMaker Usage
```csharp
// Use MockMaker for creating test doubles
var mockNode = MockMaker.CreateMockNode();
var mockWorkspace = MockMaker.CreateMockWorkspace();
```

### Test Categories
```csharp
[Test]
[Category("UnitTests")]
[Category("RegressionTests")] // Can apply multiple categories
public void BugFixAppliedCorrectly()
{
    // Test verifying specific bug fix
}
```
