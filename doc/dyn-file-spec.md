# Dynamo Graph File Format Specification (.dyn / .dyf)

> **Applies to:** Dynamo 3.0+ (JSON format)
>
> **Companion schema:** [`dyn-file-spec.json`](dyn-file-spec.json) (JSON Schema draft 2020-12)

---

## Table of Contents

1. [Overview](#1-overview)
2. [Root Object](#2-root-object)
3. [ElementResolver](#3-elementresolver)
4. [NodeInputData](#4-nodeinputdata)
5. [NodeOutputData](#5-nodeoutputdata)
6. [NodeModel (Polymorphic)](#6-nodemodel-polymorphic)
7. [PortModel](#7-portmodel)
8. [ConnectorModel](#8-connectormodel)
9. [NodeLibraryDependencyInfo](#9-nodelibrarydependencyinfo)
10. [ExtensionData](#10-extensiondata)
11. [LintingInfo](#11-lintinginfo)
12. [Bindings](#12-bindings)
13. [View Block](#13-view-block)
14. [Known ConcreteType Values](#14-known-concretetype-values)
15. [.dyn vs .dyf Differences](#15-dyn-vs-dyf-differences)
16. [GUID Format Conventions](#16-guid-format-conventions)
17. [ConcreteType Mechanism](#17-concretetype-mechanism)
18. [Versioning and Migration](#18-versioning-and-migration)
19. [Annotated Example (Minimal)](#19-annotated-example-minimal)
20. [Annotated Example (Complex)](#20-annotated-example-complex)

---

## 1. Overview

Dynamo is a visual programming application. It saves graphs as `.dyn` files (home workspace) and custom node definitions as `.dyf` files. Since Dynamo 3.0, both formats use **JSON**. Older versions (pre-3.0) used XML.

A `.dyn` file is a single JSON object containing:
- **Graph data** (model layer): nodes, connectors, dependencies, inputs/outputs, metadata
- **View data** (UI layer): node positions, canvas pan/zoom, camera, annotations, notes

Serialization is performed in two stages:
1. **Model layer** — `WorkspaceWriteConverter` serializes the `WorkspaceModel` to JSON (`src/DynamoCore/Graph/Workspaces/SerializationConverters.cs`)
2. **View layer** — `WorkspaceViewWriteConverter` appends the `View` block from the `WorkspaceViewModel` (`src/DynamoCoreWpf/ViewModels/Core/Converters/SerializationConverters.cs`)

---

## 2. Root Object

| Property | Type | Required | Condition | Description |
|----------|------|----------|-----------|-------------|
| `Uuid` | `string` (GUID, hyphenated) | Yes | — | Unique workspace identifier |
| `IsCustomNode` | `boolean` | Yes | — | `true` for `.dyf`, `false` for `.dyn` |
| `Category` | `string` | Yes* | Only when `IsCustomNode=true` | Hierarchical category path |
| `Description` | `string\|null` | Yes | — | Human-readable description |
| `Name` | `string` | Yes | — | Workspace name (derived from filename) |
| `ElementResolver` | `object` | Yes | — | Type resolution map |
| `Inputs` | `array<NodeInputData>` | Yes | — | Nodes marked as graph inputs |
| `Outputs` | `array<NodeOutputData>` | Yes | — | Nodes marked as graph outputs |
| `Nodes` | `array<NodeModel>` | Yes | — | All nodes (excluding transient) |
| `Connectors` | `array<ConnectorModel>` | Yes | — | All wire connections |
| `Dependencies` | `array<string>` | Yes | — | GUIDs of used custom node definitions |
| `NodeLibraryDependencies` | `array<NodeLibraryDependencyInfo>` | No | — | Package/file dependencies. *May be absent in files saved before Dynamo 2.0.* |
| `EnableLegacyPolyCurveBehavior` | `boolean\|null` | No | `.dyn` only | Legacy geometry flag |
| `Thumbnail` | `string\|null` | No | `.dyn` only | Base64-encoded thumbnail or empty |
| `GraphDocumentationURL` | `string\|null` | No | `.dyn` only | External documentation URI |
| `ExtensionWorkspaceData` | `array<ExtensionData>` | No | `.dyn` only | Extension-specific data |
| `Author` | `string` | No | — | Graph author name. *May be absent in files saved before Dynamo 2.0.* |
| `Linting` | `object` | No | — | Active linter state |
| `Bindings` | `array<BindingEntry>` | No | — | Execution trace data |
| `View` | `object` | Yes | — | UI/view layer data |

---

## 3. ElementResolver

Maps partial class names to their fully qualified names and containing assemblies.

```json
"ElementResolver": {
  "ResolutionMap": {
    "Color": {
      "Key": "DSCore.Color",
      "Value": "DSCoreNodes.dll"
    }
  }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `ResolutionMap` | `object` | Map from short name to `{Key, Value}` |
| `ResolutionMap.<name>.Key` | `string` | Fully qualified type name |
| `ResolutionMap.<name>.Value` | `string` | Assembly / DLL file name |

---

## 4. NodeInputData

Metadata for nodes designated as graph inputs (for Dynamo Player / Customizer).

**Source:** `src/DynamoCore/Graph/Nodes/NodeInputData.cs`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` (GUID, N format) | Yes | Node GUID |
| `Name` | `string` | Yes | Display name |
| `Type` | `string` (enum) | Yes | Input type (legacy, backward-compatible subset) |
| `Type2` | `string` (enum) | No | Input type (preferred, full set). *Added in Dynamo 2.x; absent in older files.* |
| `Value` | `string` | Yes | Value at save time (always string) |
| `Choices` | `array<string>` | No | Dropdown choices (omitted if null) |
| `MaximumValue` | `number\|null` | No | Max for number inputs |
| `MinimumValue` | `number\|null` | No | Min for number inputs |
| `StepValue` | `number\|null` | No | Step for sliders |
| `NumberType` | `string\|null` | No | `"Double"` or `"Int32"` |
| `Description` | `string\|null` | No | Tooltip text |
| `SelectedIndex` | `integer\|null` | No | Selected dropdown index |

### NodeInputTypes Enum

| Serialized Value | Enum Member | Notes |
|------------------|-------------|-------|
| `"number"` | `numberInput` | |
| `"boolean"` | `booleanInput` | |
| `"string"` | `stringInput` | |
| `"color"` | `colorInput` | |
| `"date"` | `dateInput` | |
| `"selection"` | `selectionInput` | Obsolete |
| `"hostSelection"` | `hostSelection` | Type2 only |
| `"dropdownSelection"` | `dropdownSelection` | Type2 only |

---

## 5. NodeOutputData

Metadata for nodes designated as graph outputs.

**Source:** `src/DynamoCore/Graph/Nodes/NodeOutputData.cs`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` (GUID, N format) | Yes | Node GUID |
| `Name` | `string` | Yes | Display name |
| `Type` | `string` (enum) | Yes | Output type |
| `InitialValue` | `string\|null` | Yes | Value at save time |
| `Description` | `string\|null` | No | Tooltip text |

### NodeOutputTypes Enum

| Serialized Value | Enum Member |
|------------------|-------------|
| `"integer"` | `integerOutput` |
| `"float"` | `floatOutput` |
| `"boolean"` | `booleanOutput` |
| `"string"` | `stringOutput` |
| `"unknown"` | `unknownOutput` |

---

## 6. NodeModel (Polymorphic)

Nodes are polymorphic — the `ConcreteType` field determines which additional properties are present.

**Source:** `src/DynamoCore/Graph/Nodes/NodeModel.cs`, `SerializationConverters.cs`

### Common Properties (All Node Types)

| Property | Type | JSON Order | Description |
|----------|------|------------|-------------|
| `ConcreteType` | `string` | — | Assembly-qualified type name (replaces `$type`) |
| `Id` | `string` (GUID, N format) | 1 | Unique node identifier |
| `NodeType` | `string` (enum) | 2 | Semantic type category |
| `Inputs` | `array<PortModel>` | 3 | Input ports |
| `Outputs` | `array<PortModel>` | 4 | Output ports |
| `Replication` | `string` (enum) | 6 | Lacing strategy |
| `Description` | `string` | 7 | Node description (ignored on deserialization) |

### NodeType Values

| Value | Used By |
|-------|---------|
| `"ExtensionNode"` | Default for most NodeModel subclasses (Watch, CreateList, Dropdown, Selection, etc.) |
| `"FunctionNode"` | DSFunction, DSVarArgFunction, Function (custom nodes) |
| `"CodeBlockNode"` | CodeBlockNodeModel |
| `"NumberInputNode"` | DoubleSlider, IntegerSlider, DoubleInput |
| `"BooleanInputNode"` | BoolSelector |
| `"StringInputNode"` | StringInput |
| `"ColorInputNode"` | ColorPalette |
| `"DateTimeInputNode"` | DateTimeInput |
| `"PythonScriptNode"` | PythonNode |
| `"ConvertBetweenUnitsNode"` | DynamoConvert |
| `"InputNode"` | Symbol (custom node input port definition) |
| `"OutputNode"` | Output (custom node output port definition) |
| `"FormulaNode"` | Formula (math expression evaluator) |

### LacingStrategy / Replication Enum

| Value | Description |
|-------|-------------|
| `"Disabled"` | Lacing not applicable |
| `"First"` | Use only first items |
| `"Shortest"` | Zip to shortest list |
| `"Longest"` | Zip to longest list (pad with last) |
| `"CrossProduct"` | Full cross product |
| `"Auto"` | Automatic selection |

### Type-Specific Properties

Additional properties present depending on `ConcreteType`:

#### DSFunction / DSVarArgFunction (`NodeType: "FunctionNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `FunctionSignature` | `string` | Mangled function name (e.g. `"+@var[]..[],var[]..[]"`) |

#### Function — Custom Node (`NodeType: "FunctionNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `FunctionSignature` | `string` | GUID of the custom node definition |
| `Name` | `string` | Custom node display name |
| `Description` | `string` | Custom node description |
| `Category` | `string` | Category path |

#### CodeBlockNodeModel (`NodeType: "CodeBlockNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | DesignScript source code |

#### Slider Nodes (`NodeType: "NumberInputNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `NumberType` | `string` | `"Double"` or `"Integer"` |
| `MaximumValue` | `number` | Slider max |
| `MinimumValue` | `number` | Slider min |
| `StepValue` | `number` | Slider step |
| `InputValue` | `number` | Current value |

#### Watch Node (`NodeType: "ExtensionNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `WatchWidth` | `number` | Custom display width |
| `WatchHeight` | `number` | Custom display height |

#### Dropdown Nodes (`NodeType: "ExtensionNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `SelectedIndex` | `integer` | Selected item index (-1 if none) |
| `SelectedString` | `string` | Serialized selection value |

#### Host Selection Nodes (`NodeType: "ExtensionNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `InstanceId` | `array<string>` | Selection identifiers |

#### DefineData Node (`NodeType: "ExtensionNode"`)
| Property | Type | Description |
|----------|------|-------------|
| `IsAutoMode` | `boolean` | Auto-mode flag |
| `IsList` | `boolean` | List flag |
| `DisplayValue` | `string` | Display value |

> **Extensibility note:** The node format is open-ended. Any `NodeModel` subclass can add `[JsonProperty]`-annotated properties that will be serialized. The properties above cover the built-in node types.

---

## 7. PortModel

An input or output port on a node.

**Source:** `src/DynamoCore/Graph/Nodes/PortModel.cs`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` (GUID, N format) | Yes | Unique port identifier |
| `Name` | `string` | Yes | Port label |
| `Description` | `string` | Yes | Port tooltip (maps to `ToolTip` in code) |
| `UsingDefaultValue` | `boolean` | Yes | Whether using default value |
| `Level` | `integer` | Yes | List nesting level (default: 2) |
| `UseLevels` | `boolean` | Yes | Whether list-at-level is enabled |
| `KeepListStructure` | `boolean` | Yes | Whether to preserve list structure |

```json
{
  "Id": "a775e8eb337a4c3eb5f536babb67d5df",
  "Name": "x",
  "Description": "Integer value, double value or string\n\nvar[]..[]",
  "UsingDefaultValue": false,
  "Level": 2,
  "UseLevels": false,
  "KeepListStructure": false
}
```

---

## 8. ConnectorModel

A wire connecting an output port to an input port.

**Source:** `src/DynamoCore/Graph/Connectors/ConnectorModel.cs`, `ConnectorConverter` in `SerializationConverters.cs`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Start` | `string` (GUID, N format) | Yes | Source output port GUID |
| `End` | `string` (GUID, N format) | Yes | Target input port GUID |
| `Id` | `string` (GUID, N format) | Yes | Unique connector identifier |
| `IsHidden` | `string` | No | `"True"` or `"False"` (**string, not boolean**). *May be absent in older files; defaults to `"False"`.* |

> **Important:** `IsHidden` is serialized as a string via `connector.IsHidden.ToString()`, not as a JSON boolean.

```json
{
  "Start": "a775e8eb337a4c3eb5f536babb67d5df",
  "End": "b51525b0e4b04cd79a19ade4b2506428",
  "Id": "f36934e09ec2494786410a42a00c9522",
  "IsHidden": "False"
}
```

---

## 9. NodeLibraryDependencyInfo

Package, file, or external dependencies required by nodes.

**Source:** `NodeLibraryDependencyConverter` in `SerializationConverters.cs`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Package or file name |
| `Version` | `string` | No | Semantic version (packages only) |
| `ReferenceType` | `string` (enum) | Yes | Dependency type |
| `Nodes` | `array<string>` | Yes | GUIDs of dependent nodes |

### ReferenceType Enum

| Value | Description |
|-------|-------------|
| `"Package"` | Dynamo package dependency |
| `"ZeroTouch"` | Zero-touch DLL import |
| `"DYFFile"` | Custom node definition file |
| `"External"` | External reference |
| `"NodeModel"` | NodeModel-based node from a DLL |
| `"DSFile"` | DesignScript file |

---

## 10. ExtensionData

Data stored by Dynamo extensions/plugins. Only present in `.dyn` files.

**Source:** `src/DynamoCore/Extensions/ExtensionData.cs`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `ExtensionGuid` | `string` | Yes | Extension unique identifier |
| `Name` | `string` | Yes | Extension name |
| `Version` | `string` | Yes | Extension version |
| `Data` | `object<string, string>` | Yes | Key-value data |

```json
{
  "ExtensionGuid": "28992e1d-abb9-417f-8b1b-05e053bee670",
  "Name": "Properties",
  "Version": "4.1",
  "Data": {}
}
```

---

## 11. LintingInfo

Active linter configuration snapshot.

**Source:** `LinterManagerConverter` in `SerializationConverters.cs`

| Property | Type | Description |
|----------|------|-------------|
| `activeLinter` | `string` | Linter name, or `"None"` |
| `activeLinterId` | `string` | Linter GUID |
| `warningCount` | `integer` | Warning count |
| `errorCount` | `integer` | Error count |

```json
{
  "activeLinter": "None",
  "activeLinterId": "7b75fb44-43fd-4631-a878-29f4d5d8399a",
  "warningCount": 0,
  "errorCount": 0
}
```

---

## 12. Bindings

Execution trace data for element binding (e.g. Dynamo for Revit). Only present when an engine was available at save time.

Nodes eligible for trace: `DSFunction`, `DSVarArgFunction`, `CodeBlockNodeModel`, `Function` (custom nodes), and nodes with `[RegisterForTrace]` attribute.

**Source:** `WorkspaceWriteConverter` lines 948–989

| Property | Type | Description |
|----------|------|-------------|
| `NodeId` | `string` | Node GUID |
| `Binding` | `object` | Map of callsite IDs to trace data strings |

```json
{
  "NodeId": "8c0c27a68607457898697de3d1cb4e9e",
  "Binding": {
    "callsite-id-1": "{\"serialized\":\"trace-data\"}"
  }
}
```

---

## 13. View Block

UI/view layer data added by the WPF ViewModel during save.

**Source:** `src/DynamoCoreWpf/ViewModels/Core/Converters/SerializationConverters.cs`

| Property | Type | Description |
|----------|------|-------------|
| `Dynamo` | `object` | Application state snapshot |
| `Camera` | `object` | 3D preview camera |
| `ConnectorPins` | `array` | Pins on connectors. *Added in Dynamo 2.13; absent in older files.* |
| `NodeViews` | `array` | Per-node positions and flags |
| `Annotations` | `array` | Groups and serialized notes |
| `X` | `number` | Canvas pan X |
| `Y` | `number` | Canvas pan Y |
| `Zoom` | `number` | Canvas zoom level |

### 13a. View.Dynamo

| Property | Type | Description |
|----------|------|-------------|
| `ScaleFactor` | `number` | Geometry scale factor (default: 1.0) |
| `HasRunWithoutCrash` | `boolean` | Graph executed successfully at least once |
| `IsVisibleInDynamoLibrary` | `boolean` | Custom node library visibility |
| `Version` | `string` | Dynamo version at save time |
| `RunType` | `string` | `"Automatic"`, `"Manual"`, or `"Periodic"` |
| `RunPeriod` | `string` | Milliseconds between auto-runs (as string) |

### 13b. View.Camera

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Camera name (e.g. `"_Background Preview"`) |
| `EyeX`, `EyeY`, `EyeZ` | `number` | Camera eye position |
| `LookX`, `LookY`, `LookZ` | `number` | Camera look direction |
| `UpX`, `UpY`, `UpZ` | `number` | Camera up vector |

### 13c. View.ConnectorPins

| Property | Type | Description |
|----------|------|-------------|
| `ConnectorGuid` | `string` | Parent connector GUID |
| `Left` | `number` | Pin X position |
| `Top` | `number` | Pin Y position |

### 13d. View.NodeViews

**Source:** `src/DynamoCoreWpf/ViewModels/Core/NodeViewModel.cs`

| Property | Type | JSON Order | Description |
|----------|------|------------|-------------|
| `Id` | `string` (GUID, N format) | 1 | Node GUID |
| `Name` | `string` | 2 | Display name |
| `IsSetAsInput` | `boolean` | 3 | Marked as graph input |
| `IsSetAsOutput` | `boolean` | 4 | Marked as graph output |
| `Excluded` | `boolean` | 5 | Frozen/excluded (maps to `IsFrozenExplicitly`) |
| `ShowGeometry` | `boolean` | 6 | Render in 3D preview (maps to `IsVisible`) |
| `X` | `number` | 7 | Canvas X position |
| `Y` | `number` | 8 | Canvas Y position |
| `UserDescription` | `string\|null` | — | User note (omitted when null) |

### 13e. View.Annotations

Groups and notes. **Notes are serialized as annotations** with an empty `Nodes` array and optional `PinnedNode`.

**Source:** `AnnotationViewModelConverter` in `DynamoCoreWpf/ViewModels/Core/Converters/SerializationConverters.cs`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` (GUID, N format) | Annotation/note identifier |
| `Title` | `string` | Group title or note text |
| `DescriptionText` | `string\|null` | Group description |
| `IsExpanded` | `boolean` | Expanded/collapsed state (default: `true`). *May be absent in older files; defaults to `true`.* |
| `WidthAdjustment` | `number` | Manual width adjustment |
| `HeightAdjustment` | `number` | Manual height adjustment |
| `UserSetWidth` | `number` | User-specified width |
| `UserSetHeight` | `number` | User-specified height |
| `Nodes` | `array<string>` | GUIDs of member nodes (empty for notes) |
| `HasNestedGroups` | `boolean` | Contains nested groups |
| `Left` | `number` | X position |
| `Top` | `number` | Y position |
| `Width` | `number` | Group width |
| `Height` | `number` | Group height |
| `FontSize` | `number` | Title font size |
| `GroupStyleId` | `string` | Style GUID |
| `InitialTop` | `number` | Initial top before adjustments |
| `InitialHeight` | `number` | Initial height before adjustments |
| `TextblockHeight` | `number` | Title text block height |
| `IsOptionalInPortsCollapsed` | `boolean` | Optional input ports collapsed |
| `IsUnconnectedOutPortsCollapsed` | `boolean` | Unconnected output ports collapsed |
| `HasToggledOptionalInPorts` | `boolean` | User toggled optional inputs |
| `HasToggledUnconnectedOutPorts` | `boolean` | User toggled unconnected outputs |
| `Background` | `string` | ARGB hex color (e.g. `"#FFC1D676"`) |
| `PinnedNode` | `string` | GUID of pinned node (notes only, optional) |

---

## 14. Known ConcreteType Values

The `ConcreteType` field identifies the node class. Below are the well-known types and their additional properties:

| ConcreteType | NodeType | Extra Properties |
|---|---|---|
| `Dynamo.Graph.Nodes.ZeroTouch.DSFunction, DynamoCore` | `FunctionNode` | `FunctionSignature` |
| `Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction, DynamoCore` | `FunctionNode` | `FunctionSignature` |
| `Dynamo.Graph.Nodes.CustomNodes.Function, DynamoCore` | `FunctionNode` | `FunctionSignature` (GUID), `Name`, `Description`, `Category` |
| `Dynamo.Graph.Nodes.CodeBlockNodeModel, DynamoCore` | `CodeBlockNode` | `Code` |
| `CoreNodeModels.Input.DoubleSlider, CoreNodeModels` | `NumberInputNode` | `NumberType`, `MaximumValue`, `MinimumValue`, `StepValue`, `InputValue` |
| `CoreNodeModels.Input.IntegerSlider, CoreNodeModels` | `NumberInputNode` | `NumberType`, `MaximumValue`, `MinimumValue`, `StepValue`, `InputValue` |
| `CoreNodeModels.Input.DoubleInput, CoreNodeModels` | `NumberInputNode` | `NumberType`, `InputValue` |
| `CoreNodeModels.Input.BoolSelector, CoreNodeModels` | `ExtensionNode` | `InputValue` |
| `CoreNodeModels.Input.StringInput, CoreNodeModels` | `ExtensionNode` | `InputValue` |
| `CoreNodeModels.Watch, CoreNodeModels` | `ExtensionNode` | `WatchWidth`, `WatchHeight` |
| `CoreNodeModels.Input.FileObject, CoreNodeModels` | `ExtensionNode` | *(none)* |
| `CoreNodeModels.CreateList, CoreNodeModels` | `ExtensionNode` | *(variable input ports)* |
| `CoreNodeModels.DefineData, CoreNodeModels` | `ExtensionNode` | `IsAutoMode`, `IsList`, `DisplayValue` |
| `CoreNodeModels.CurveMapperNodeModel, CoreNodeModels` | `ExtensionNode` | `*ControlPointData*` properties |
| Dropdown subclasses | `ExtensionNode` | `SelectedIndex`, `SelectedString` |
| Selection subclasses | `ExtensionNode` | `InstanceId` |
| `CoreNodeModels.Input.Formula, CoreNodeModels` | `FormulaNode` | `FormulaString` |

> The format is extensible — packages and host integrations can introduce new `ConcreteType` values with custom properties.

---

## 15. .dyn vs .dyf Differences

Both `.dyn` and `.dyf` files share the same JSON schema. The differences:

| Aspect | `.dyn` (Home Workspace) | `.dyf` (Custom Node) |
|--------|-------------------------|----------------------|
| `IsCustomNode` | `false` | `true` |
| `Category` | Absent | Present (required) |
| `EnableLegacyPolyCurveBehavior` | Present | Absent |
| `Thumbnail` | Present | Absent |
| `GraphDocumentationURL` | Present | Absent |
| `ExtensionWorkspaceData` | Present | Absent |
| `Bindings` | Present (if engine available) | Absent |
| `Uuid` | Workspace GUID | Custom node definition ID |
| Typical node content | Any nodes | Contains `Symbol` (input) and `Output` nodes |

---

## 16. GUID Format Conventions

Dynamo uses two GUID formats:

| Format | Pattern | Used For |
|--------|---------|----------|
| **Hyphenated** | `8731c011-66bc-4cc2-80a8-712e7c75bcfe` | Root `Uuid` only |
| **N format** (no hyphens) | `8c0c27a68607457898697de3d1cb4e9e` | Node IDs, Port IDs, Connector IDs, Annotation IDs, all View block GUIDs |

The N format is produced by `Guid.ToString("N")` in the serialization code.

---

## 17. ConcreteType Mechanism

Dynamo uses Newtonsoft.Json (JSON.NET) with `TypeNameHandling.Auto` for serialization. JSON.NET emits a `$type` discriminator for polymorphic types. After serialization, a regex replacement converts `$type` to `ConcreteType`:

```csharp
// SerializationExtensions.cs
var rgx2 = new Regex(@"\$type");
result = rgx2.Replace(result, "ConcreteType");
```

On deserialization, the reverse replacement is applied before JSON.NET processes the string.

The `ConcreteType` value is an assembly-qualified type name: `"Namespace.ClassName, AssemblyName"`.

If a type cannot be resolved during deserialization, a **DummyNode** is created that preserves the original JSON content so it can be re-serialized without data loss.

---

## 18. Versioning and Migration

- The Dynamo version is stored in `View.Dynamo.Version`
- `IntegerSlider` types are migrated: on serialization, `IntegerSlider64Bit` is written as `IntegerSlider` for backward compatibility. On deserialization, `IntegerSlider` is promoted to `IntegerSlider64Bit`.
- Type names can be resolved via `[AlsoKnownAs]` attributes for backward compatibility with older type names
- Legacy XML `.dyn` files (pre-3.0) are detected and migrated transparently

### Properties Absent in Older Files

The following properties were added in later Dynamo versions and may be absent when reading files saved by earlier versions. Consumers should treat them as optional and use the listed defaults:

| Property | Location | Added In | Default When Absent |
|----------|----------|----------|---------------------|
| `NodeLibraryDependencies` | Root | ~Dynamo 2.0 | `[]` (empty array) |
| `Author` | Root | ~Dynamo 2.0 | `""` (empty string) |
| `Linting` | Root | ~Dynamo 2.13 | `null` (no linter active) |
| `Type2` | `NodeInputData` | ~Dynamo 2.x | Same as `Type` value |
| `ConnectorPins` | `View` block | ~Dynamo 2.13 | `[]` (empty array) |
| `IsHidden` | `ConnectorModel` | ~Dynamo 2.x | `"False"` |
| `IsExpanded` | `AnnotationViewInfo` | ~Dynamo 2.x | `true` |
| `HasNestedGroups` | `AnnotationViewInfo` | ~Dynamo 2.6 | `false` |
| `GroupStyleId` | `AnnotationViewInfo` | ~Dynamo 2.13 | `""` (empty) |
| `UserDescription` | `NodeViewInfo` | ~Dynamo 3.x | `null` (omitted) |
| `Excluded` | `NodeViewInfo` | ~Dynamo 2.6 | `false` |

### Deprecated / Legacy Properties

| Property | Location | Status | Notes |
|----------|----------|--------|-------|
| `NodeInputData.Type` | `NodeInputData` | Legacy | Superseded by `Type2`; kept for backward compatibility. `Type2` includes additional values (`hostSelection`, `dropdownSelection`). |
| `NodeInputData.Type.selection` | `NodeInputData.Type` enum | Obsolete | Marked `[Obsolete]` in source code. Use `hostSelection` or `dropdownSelection` via `Type2` instead. |
| `IntegerSlider` | `ConcreteType` | Migration alias | Internally deserialized as `IntegerSlider64Bit`; re-serialized as `IntegerSlider` for backward compatibility. |

---

## 19. Annotated Example (Minimal)

A minimal `.dyn` file with a single number slider node:

```json
{
  "Uuid": "a017e1cc-8acd-45b5-af80-a2707ffa6d70",
  "IsCustomNode": false,
  "Description": "",
  "Name": "MinimalExample",
  "ElementResolver": {
    "ResolutionMap": {}
  },
  "Inputs": [],
  "Outputs": [],
  "Nodes": [
    {
      "ConcreteType": "CoreNodeModels.Input.DoubleSlider, CoreNodeModels",
      "NumberType": "Double",
      "MaximumValue": 100.0,
      "MinimumValue": 0.0,
      "StepValue": 1.0,
      "Id": "8c0c27a68607457898697de3d1cb4e9e",
      "NodeType": "NumberInputNode",
      "Inputs": [],
      "Outputs": [
        {
          "Id": "a775e8eb337a4c3eb5f536babb67d5df",
          "Name": "",
          "Description": "Double",
          "UsingDefaultValue": false,
          "Level": 2,
          "UseLevels": false,
          "KeepListStructure": false
        }
      ],
      "Replication": "Disabled",
      "Description": "Produces numeric values",
      "InputValue": 50.0
    }
  ],
  "Connectors": [],
  "Dependencies": [],
  "NodeLibraryDependencies": [],
  "EnableLegacyPolyCurveBehavior": true,
  "Thumbnail": "",
  "GraphDocumentationURL": null,
  "ExtensionWorkspaceData": [
    {
      "ExtensionGuid": "28992e1d-abb9-417f-8b1b-05e053bee670",
      "Name": "Properties",
      "Version": "3.2",
      "Data": {}
    }
  ],
  "Author": "None provided",
  "Linting": {
    "activeLinter": "None",
    "activeLinterId": "7b75fb44-43fd-4631-a878-29f4d5d8399a",
    "warningCount": 0,
    "errorCount": 0
  },
  "Bindings": [],
  "View": {
    "Dynamo": {
      "ScaleFactor": 1.0,
      "HasRunWithoutCrash": true,
      "IsVisibleInDynamoLibrary": true,
      "Version": "3.3.0.5104",
      "RunType": "Automatic",
      "RunPeriod": "1000"
    },
    "Camera": {
      "Name": "_Background Preview",
      "EyeX": -17.0,
      "EyeY": 24.0,
      "EyeZ": 50.0,
      "LookX": 12.0,
      "LookY": -13.0,
      "LookZ": -58.0,
      "UpX": 0.0,
      "UpY": 1.0,
      "UpZ": 0.0
    },
    "ConnectorPins": [],
    "NodeViews": [
      {
        "Id": "8c0c27a68607457898697de3d1cb4e9e",
        "Name": "Number Slider",
        "IsSetAsInput": false,
        "IsSetAsOutput": false,
        "Excluded": false,
        "ShowGeometry": true,
        "X": 100.0,
        "Y": 200.0
      }
    ],
    "Annotations": [],
    "X": 0.0,
    "Y": 0.0,
    "Zoom": 1.0
  }
}
```

---

## 20. Annotated Example (Complex)

A `.dyn` file fragment showing a code block connected to a DSFunction, with a group annotation:

```json
{
  "Uuid": "19433588-cb44-439f-a3b5-bdc676dcb105",
  "IsCustomNode": false,
  "Description": null,
  "Name": "ComplexExample",
  "ElementResolver": {
    "ResolutionMap": {
      "DSCore.Data": {
        "Key": "DSCore.Data",
        "Value": "DSCoreNodes.dll"
      }
    }
  },
  "Inputs": [],
  "Outputs": [],
  "Nodes": [
    {
      "ConcreteType": "Dynamo.Graph.Nodes.CodeBlockNodeModel, DynamoCore",
      "Id": "9dca6adcdcf2436a931743a0af5195bc",
      "NodeType": "CodeBlockNode",
      "Inputs": [],
      "Outputs": [
        {
          "Id": "a007ba6a79be46a1867ea53b5034a123",
          "Name": "value",
          "Description": "...",
          "UsingDefaultValue": false,
          "Level": 2,
          "UseLevels": false,
          "KeepListStructure": false
        }
      ],
      "Replication": "Disabled",
      "Description": "Allows for DesignScript code to be authored directly",
      "Code": "\"hello world\";"
    },
    {
      "ConcreteType": "Dynamo.Graph.Nodes.ZeroTouch.DSFunction, DynamoCore",
      "Id": "aa367b7b22c5492ebe309690c8a45960",
      "NodeType": "FunctionNode",
      "Inputs": [
        {
          "Id": "d50dec447a904b7a8e84beb65aa39eb0",
          "Name": "json",
          "Description": "A JSON string\n\nstring",
          "UsingDefaultValue": false,
          "Level": 2,
          "UseLevels": false,
          "KeepListStructure": false
        }
      ],
      "Outputs": [
        {
          "Id": "1cf7f91f6e784fd4bfe2d552d42d1fbe",
          "Name": "result",
          "Description": "The result type depends on the content of the input string.",
          "UsingDefaultValue": false,
          "Level": 2,
          "UseLevels": false,
          "KeepListStructure": false
        }
      ],
      "FunctionSignature": "DSCore.Data.ParseJSON@string",
      "Replication": "Auto",
      "Description": "Parse converts an arbitrary JSON string to a value."
    }
  ],
  "Connectors": [
    {
      "Start": "a007ba6a79be46a1867ea53b5034a123",
      "End": "d50dec447a904b7a8e84beb65aa39eb0",
      "Id": "87046c304787435fa4e7d9efef95ae00",
      "IsHidden": "False"
    }
  ],
  "Dependencies": [],
  "NodeLibraryDependencies": [],
  "EnableLegacyPolyCurveBehavior": true,
  "Thumbnail": null,
  "GraphDocumentationURL": null,
  "ExtensionWorkspaceData": [
    {
      "ExtensionGuid": "28992e1d-abb9-417f-8b1b-05e053bee670",
      "Name": "Properties",
      "Version": "4.1",
      "Data": {}
    }
  ],
  "Author": "None provided",
  "Linting": {
    "activeLinter": "None",
    "activeLinterId": "7b75fb44-43fd-4631-a878-29f4d5d8399a",
    "warningCount": 0,
    "errorCount": 0
  },
  "Bindings": [],
  "View": {
    "Dynamo": {
      "ScaleFactor": 1.0,
      "HasRunWithoutCrash": true,
      "IsVisibleInDynamoLibrary": true,
      "Version": "4.1.0.3057",
      "RunType": "Automatic",
      "RunPeriod": "1000"
    },
    "Camera": {
      "Name": "_Background Preview",
      "EyeX": -17.0,
      "EyeY": 24.0,
      "EyeZ": 50.0,
      "LookX": 12.0,
      "LookY": -13.0,
      "LookZ": -58.0,
      "UpX": 0.0,
      "UpY": 1.0,
      "UpZ": 0.0
    },
    "ConnectorPins": [],
    "NodeViews": [
      {
        "Id": "9dca6adcdcf2436a931743a0af5195bc",
        "Name": "Code Block",
        "IsSetAsInput": false,
        "IsSetAsOutput": false,
        "Excluded": false,
        "ShowGeometry": true,
        "X": 55.0,
        "Y": 130.0
      },
      {
        "Id": "aa367b7b22c5492ebe309690c8a45960",
        "Name": "Data.ParseJSON",
        "IsSetAsInput": false,
        "IsSetAsOutput": false,
        "Excluded": false,
        "ShowGeometry": true,
        "X": 400.0,
        "Y": 130.0
      }
    ],
    "Annotations": [
      {
        "Id": "fb0e56f017914521b0ac990c45999ebb",
        "Title": "JSON Parsing Example",
        "DescriptionText": null,
        "IsExpanded": true,
        "WidthAdjustment": 0.0,
        "HeightAdjustment": 0.0,
        "UserSetWidth": 0.0,
        "UserSetHeight": 0.0,
        "Nodes": [
          "9dca6adcdcf2436a931743a0af5195bc",
          "aa367b7b22c5492ebe309690c8a45960"
        ],
        "HasNestedGroups": false,
        "Left": 45.0,
        "Top": 14.0,
        "Width": 685.0,
        "Height": 388.0,
        "FontSize": 36.0,
        "GroupStyleId": "00000000-0000-0000-0000-000000000000",
        "InitialTop": 130.0,
        "InitialHeight": 145.0,
        "TextblockHeight": 106.0,
        "IsOptionalInPortsCollapsed": false,
        "IsUnconnectedOutPortsCollapsed": false,
        "HasToggledOptionalInPorts": false,
        "HasToggledUnconnectedOutPorts": false,
        "Background": "#FFC1D676"
      }
    ],
    "X": -15.0,
    "Y": 129.0,
    "Zoom": 0.49
  }
}
```

---

## Source Code References

| Component | File Path |
|-----------|-----------|
| Workspace write converter | `src/DynamoCore/Graph/Workspaces/SerializationConverters.cs` (line 797) |
| Workspace read converter | `src/DynamoCore/Graph/Workspaces/SerializationConverters.cs` (line 453) |
| Node read converter | `src/DynamoCore/Graph/Workspaces/SerializationConverters.cs` (line 38) |
| Connector converter | `src/DynamoCore/Graph/Workspaces/SerializationConverters.cs` (line 1242) |
| Serialization extensions | `src/DynamoCore/Graph/Workspaces/SerializationExtensions.cs` |
| View info types | `src/DynamoCore/Graph/Workspaces/WorkspaceModel.cs` (line 43) |
| View block writer | `src/DynamoCoreWpf/ViewModels/Core/Converters/SerializationConverters.cs` |
| Annotation writer | `src/DynamoCoreWpf/ViewModels/Core/Converters/SerializationConverters.cs` (line 103) |
| NodeModel | `src/DynamoCore/Graph/Nodes/NodeModel.cs` |
| PortModel | `src/DynamoCore/Graph/Nodes/PortModel.cs` |
| ConnectorModel | `src/DynamoCore/Graph/Connectors/ConnectorModel.cs` |
| NodeInputData | `src/DynamoCore/Graph/Nodes/NodeInputData.cs` |
| NodeOutputData | `src/DynamoCore/Graph/Nodes/NodeOutputData.cs` |
| ExtensionData | `src/DynamoCore/Extensions/ExtensionData.cs` |
