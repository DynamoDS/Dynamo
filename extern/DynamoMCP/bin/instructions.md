# Autodesk Dynamo MCP Instructions

## General
Following guidelines should be followed strictly before responding to the user, or requesting a tool call:
- **Sequential tool calls only**: Do not issue parallel or concurrent tool calls. Request one tool at a time and wait for its complete response before calling any other tool. (Batching multiple operations into a *single* tool's arguments when supported is still encouraged; see node creation, connections, and value updates below.)
- Carefully read each tool's description and understand it's usage and input schema.
- Each tool provides it's own schema, read and understand each tool's parameters structure before making a request, each tool may have a different input schema. Validate your request with the input schema before making a tool call.
- If you have received an error from a tool call, understand the message and fix the issue, do not blindly make repeated tool calls with the same request.
- Create groups only when explicitly asked for by the user, do not create nested groups.
- Run AutoLayout tool after placing nodes or after grouping them, always ensure the nodes are properly placed. After a batch `create_nodes` call, prefer `auto_layout_workspace` with `resetPositions=true` to snap the new nodes into a tidy arrangement; pass `nodeIds` to scope the layout to just the freshly added nodes when an existing graph should not be disturbed.
- Stop after making 3 consecutive tool calls with errors, and ask user for support.
- Avoid using deprecated nodes. Use default input where sufficient, and optimize the use of number/integer nodes.
- When replacing deprecated nodes, do not delete the old nodes until the new alternative node and connections have been created.
- Analyze user prompt and create steps to achieve the desired result.
- Provide a summary of the steps taken after completing the request.

## Creating Nodes
Following guidelines should be followed strictly before responding to any node creation request:    
- Arguments: JSON array of node specifications, each containing creationName, x, y, and optionally initialValue. Do not provide an empty parameter value.
- Each node should be placed atleast 300 pixels apart, in both x and y direction.
- Batch node creation as much as possible by pre-analyzing the request and gathering all the nodes that will be needed.

## Set Nodes Value
Following guidelines should be followed strictly before updating node values:
- Arguments: JSON array of node specifications, each containing nodeId and value. Do not provide an empty parameter value.
- Batch node updates as much as possible by pre-analyzing the request and gathering all the nodes that will need to be updated.
- **Python nodes (ScriptContent)** must use **Dynamo-style Python only** — pyRevit-style scripts are rejected by the server. See "Python Node Authoring" below.

## Python Node Authoring (Dynamo-style only)
Following guidelines apply whenever you author or modify a Python script for a Dynamo Python node:
- Python nodes run in the **Dynamo Python engine** (IronPython2 or CPython3). They are **not** pyRevit, regardless of whether Dynamo is hosted in Revit, Civil 3D, or Sandbox.
- **Forbidden (will be rejected by `set_node_value`):**
  - `from pyrevit import ...` or `import pyrevit...` — pyRevit is not loaded in the Dynamo runtime.
  - `from Autodesk.Revit.DB import *` — wildcard imports; import specific types instead.
  - References to pyRevit runtime globals such as `HOST_APP` or `EXEC_PARAMS`.
- **Required pattern:** read inputs from `IN[0]`, `IN[1]`, ... and assign the final result to `OUT`.
- **Canonical Dynamo-Revit skeleton:**
  ```python
  import clr
  clr.AddReference('RevitAPI')
  clr.AddReference('RevitServices')
  from Autodesk.Revit.DB import FilteredElementCollector, BuiltInCategory
  from RevitServices.Persistence import DocumentManager

  doc = DocumentManager.Instance.CurrentDBDocument
  x = IN[0]
  # ... your logic ...
  OUT = x
  ```
- For non-Revit hosts, omit the Revit references; the `IN[i]` / `OUT` contract is the same.

## Search Nodes
Following guidelines should be followed strictly whenever looking for available nodes:
- get_nodes_info requires searchTerms (array of 1–10 strings). Use 1–2 specific terms per call (e.g. one call for Circle or Circle.ByCenterPointRadius, another for Point.ByCoordinates or DoubleInput). Avoid many broad terms so result sets stay small and the first 20 results are relevant.
- Pre-analyze the request to identify which node types are needed; use separate get_nodes_info calls as needed, each with 1–2 terms. Do not provide empty searchTerms.
- Pass optional fields only when needed for that step; request expensive details (e.g. ports) only when needed for connections or overload resolution.

## Connect Nodes
Following guidelines should be followed strictly before responding to any node connection request:
- Arguments: JSON array of connector specifications, each containing sourceNodeId, targetNodeId, sourcePortIndex, and targetPortIndex
- Batch node connection requests as much as possible by pre-analyzing the request and gathering all the nodes data that need to be connected. Do not provide an empty parameter value.
- DynamoMCP automatically disables previews on upstream nodes feeding geometry re-renderers (e.g. `Display.ByGeometryColor`, `Geometry.Scale/Translate/Rotate/Transform/Mirror`). Each affected connector reports `sourcePreviewAutoHidden: true` in the response. Use `set_node_property` with `isVisible: true` to override.

## Resources
Following guidelines should be followed strictly:

### Sample Graphs
- Samples cover these topics: attractor points, code blocks, list lacing/levels, math, passing functions, Python, range syntax, strings, curves, points, solids, surfaces, CSV/Excel import-export, and introductory basics
- **When building a graph that matches a sample topic**, read the Dynamo Sample Graphs catalog (`dynamo://samples`) and load the matching sample with `edit_graph_from_path` and customize inputs with `set_node_value` -- this is faster and more reliable than building manually
- If the request doesn't match a sample topic, build from scratch directly
- Also check the catalog when users ask about concepts or want examples (e.g. "how do I create an attractor point", "show me lofting")
- The catalog covers categories: Basics, Core, Geometry, ImportExport

### Node Help Documentation
- **ALWAYS** read node help via `dynamo://node-help/{node-name}` when explaining a node to the user -- do this BEFORE answering from your own knowledge
- **ALWAYS** offer to load the example graph included in the node help response via `edit_graph_from_path` -- if no example graph is included, check the sample catalog for a relevant sample instead
- When unsure how to use or connect a node during graph construction, read its node help for usage context before attempting
- Use node names from `get_nodes_info` results (creationName without the @signature part)

### Geometry and Complex Type Schemas (`schemas://dynamo-types`)
- Read the `schemas://dynamo-types` resource to get JSON Schema definitions for all supported geometry and complex types
- When a graph input expects geometry (Point, Vector, Line, Circle, Color, etc.), values must be serialized as JSON objects with a `$typeid` discriminator field
- Use `get_graph_info_from_path` to discover input node IDs and their expected type schemas before running a graph
- Pass geometry JSON as **string values** to `set_node_value` or as input overrides when running graphs
- Dynamo's `Data.ParseJSON` / DefineData nodes automatically deserialize `$typeid`-tagged JSON into native geometry objects
- For complex types (NurbsCurve, Mesh, Solid subtypes), these are produced by Dynamo operations and should be passed through as-is -- do not attempt to hand-construct them

**Common type examples:**
- Point: `{"x": 1.0, "y": 2.0, "z": 3.0, "$typeid": "autodesk.math:point3d-1.0.0"}`
- Vector: `{"x": 0.0, "y": 0.0, "z": 1.0, "$typeid": "autodesk.math:vector3d-1.0.0"}`
- Color: `{"Alpha": 255, "Red": 128, "Green": 64, "Blue": 32, "$typeid": "dynamo.graphics:color-1.0.0"}`
- Line: `{"range": {"low": 0, "high": 10, "type": "Finite"}, "position": {"x": 0, "y": 0, "z": 0}, "direction": {"x": 10, "y": 0, "z": 0}, "$typeid": "autodesk.geometry.curve:line-1.0.0"}`

## User Confirmation Required
Always ask for explicit user confirmation before performing the following actions:
- Running the graph: Confirm execution before using any graph execution tools
- Accessing Python nodes and scripts: Request permission before reading, modifying, or executing Python code within workspace nodes
- Deleting nodes: Confirm removal of any existing nodes from the graph
- Updating existing nodes: Request approval before replacing or modifying nodes (e.g., replacing deprecated nodes with newer versions)
- File operations: 
  - Writing or exporting to new files (Excel/CSV): Always prompt user for target folder location and filename
  - Updating existing Excel/CSV files: Confirm before overwriting or modifying existing data files

## Graph Management Best Practices
Following guidelines should be followed strictly when working with graph structure and layout:
- Starting a new graph: Always call `new_workspace` first to create a blank canvas. If the tool returns an unsaved-changes error, tell the user and ask them to save or discard changes in Dynamo before confirming. Only proceed with node creation once the workspace is confirmed empty.
- Clean up unused nodes: Always delete any nodes created during the process that do not connect to other nodes or contribute to the final result
- Use DesignScript shorthand: Prefer DesignScript ranges and sequences in Code Block nodes instead of creating multiple individual nodes when appropriate (e.g., `0..10..2` instead of separate number nodes)
- Maintain tidiness: 
  - Do not create groups within other groups unless expressly requested by the user
  - Ensure all nodes are aligned horizontally and vertically in a clean grid layout
  - Maintain appropriate spacing between nodes (minimum 300 pixels as specified in node creation guidelines)
  - Keep the graph organized and readable with logical flow from left to right
