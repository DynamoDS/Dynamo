## In Depth

The Define Data node validates the data type of incoming data. It can be used to ensure local data is of the desired type and is also designed to be used as an input or output node, declaring the type of data a graph expects or provides. The node supports a selection of commonly used Dynamo data types, for example 'String', 'Point', or 'Boolean'. The full list of supported data types is available in the drop-down menu of the node.

### Behavior
The node validates the data coming in from the input port based on the setting of the drop-down menu and the **List** toggle (see below for details). If the validation is successful, the output of the node is the same as the input. If the validation is not successful, the node will enter a warning state with a null output.

The node has four inputs:
- The "**>**" input - Connect to an upstream node to validate the type of its data.
- The **drop-down** menu - Shows the expected data type. When the form is unlocked, set a data type for validation. When the form is locked, the data type is chosen automatically based on incoming data. Data is valid if its type matches the shown type exactly or if its type is a child of the shown type (e.g. If the drop-down is set to "Curve", objects of type "Rectangle", "Line", etc. are valid).
- The **List** toggle - When on, the node expects incoming data to be a single flat list containing items of a valid data type (see above). When off, the node expects a single item of a valid data type.
- The **Lock** toggle - When off/unlocked, the drop-down menu and the **List** toggle controls accept user input to set the type of data expected. When on/locked, the node will validate* the incoming data, disable the controls, and set their value based on the data connected to the node's input port.

### Use as an input node
When set as an input ("Is Input" in the node's context menu) the node can optionally use upstream nodes to set the default value for the input. A run of the graph will cache the Define Data node's value for use when running the graph externally, for example with the Engine Node.

---

## Example File
In the example below, the first group of "DefineData" nodes have an unlocked UI. The node correctly validates the Number input provided while rejecting the String input. The second group contains a node with locked UI. The node automatically adjusts the drop-down and the **List** toggle to match the input, in this case a list of integers.

![Define_Data](./CoreNodeModels.DefineData_img.jpg)
    