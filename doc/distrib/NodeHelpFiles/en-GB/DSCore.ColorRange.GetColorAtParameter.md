## In Depth
GetColorAtParameter takes an input 2D color range, and returns a list of colors at specified UV parameters in the range 0 to 1. In the example below, we first create a 2D Color Range using a ByColorsAndParameters node with a list of colors and list of parameters to set the range. A code block is used to generate a range of numbers between 0 and 1, which is used as the u and v inputs in a UV.ByCoordinates node. The lacing of this node is set to cross product. A set of cubes is created in a similar manner, which a Point.ByCoordinates node with cross product lacing used to created an array of cubes. We then use a Display.ByGeometryColor node with the array of cubes and the list of colors obtained from the GetColorAtParameter node.
___
## Example File

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

