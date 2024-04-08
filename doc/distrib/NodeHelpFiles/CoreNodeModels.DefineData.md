## In Depth

The Define Data Node is designed to work as in input node, while validating the data type of the incoming data (type can be 'string', 'point', 'boolean' or any data type that is currently supported by Dynamo. For the full list of supported data types, expand the Dropdown menu of the node). It uses upstream nodes to set default values to make setting the type data easier as well as making the graphs functional when the nodes are used.

The node has 4 inputs:
- the node '**>**' Input - can be connected to any downstream node to validate the input of
- the **Dropdown** menu
- the '**List**' toggle 
- the '**Lock**' toggle

The node has two distinct modes in terms of behaviour - the 'Manua' and 'Auto' mode. This are defined by the state of the 'Lock' toggle - locked is for 'Manual', whereas unlocked is for 'Auto' mode. 

### Manual mode

Set the **Lock** to locked position. The user sets the type of data explicitly by interacting with the **Dropdown** and **List** toggle. The node validates the data. If the validation is successful, the output of the node is the same as the input. If the validation is not successful, the node will enter a **Warning** state with a null output.

### Auto mode

Set the **Lock** to unlocked position. The downstream data is being automaticall validated. If the type of data is valid*, the node will automatically adjust the **Dropdown** and **List** toggles to match the input  data type. If the validation process is successful, the output of the node is the same as the input. If the validation process is not successful, the node will enter a **Warning** state with a null output.

* data validity - the data is recognized by Dynamo (any of the **Dropdown** menu values) and the data is either a single objet or a List of homogeneous values, or a List of values that belong to the same hierarchical tree.

In the example below, the first group of 'DefineData' nodes is in 'Manual' 'single value' mode - the node correctly validates the Number input provided, while rejecting the String input. The second group showscases a node in 'Auto' mode - the node automatically adjusts the 'Dropdown' and 'List' toggles to the desired inputs - in this case, list of integers.

---

## Example File

![Define_Data](./CoreNodeModels.DefineData_img.jpg)
    