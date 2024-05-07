## In Depth

The Define Data node validates the data type of incoming data. It can be used to ensure local data is of the desired type and is also designed to be used as an input or output node, declaring the type of data a graph expects or provides. Currently the node supports a selection of commonly used Dynamo data types, for example 'String', 'Point', or 'Boolean'. For the full list of supported data types, expand the dropdown menu of the node. 

### Behavior
The node validates the data coming in from the input port based on the setting of the **Data Type** dropdown and **List** toggle (see below for details). If the validation is successful, the output of the node is the same as the input. If the validation is not successful, the node will enter a **Warning** state with a null output.

The node has four controls:
- **>** input - Connect to an upstream node to validate its data type.
- **Data Type** dropdown - When the form is unlocked, set a data type for validation. When the form is locked, the data type is chosen based on incoming data.
- **List** toggle - When enabled, the node expects incoming data to be a single flat list containing items of the selected data type. When disabled, the node expects a single item of the selected data type.
- **Lock** toggle - When disabled, the **Data Type** dropdown and **List** toggle controls accept user input to set the type of data expected. When enabled, the node will validate* the incoming data, disable the controls, and set their value based on the data connected to the node's input port.

* Data validity - the data is recognized by Dynamo (any of the **Data Type** dropdown values) and the data is either a single object or a list of homogeneous values, or a list of values with a common, supported, parent type.

When set as an input the node can optionally use upstream nodes to set the default value for the input. A run of the graph will cache the Define Data node's value for use when running the graph externally, for example with the Engine Node.

In the example below, the first group of 'DefineData' nodes have an unlocked UI - the node correctly validates the Number input provided, while rejecting the String input. The second group showcases a node with locked UI - the node automatically adjusts the **Data Type** dropdown and **List** toggle to match the input - in this case a list of integers.

---

## Example File

![Define_Data](./CoreNodeModels.DefineData_img.jpg)
    