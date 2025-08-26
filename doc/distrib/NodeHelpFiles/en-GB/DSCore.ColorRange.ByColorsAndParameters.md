## In Depth
ByColorsAndParameters creates an  2D color range from a list of input colors and a corresponding list of specified UV parameters in the range 0 to 1. In the example below, we use a code block to create three different colors (in this case simply green, red, and blue) and to combine them into a list. We use a separate code block to create three UV parameters, one for each color. These two lists are used as inputs to a ByColorsAndParameters node. We use a subsequent GetColorAtParameter node, along with a Display.ByGeometryColor node to visualize the 2D color range across a set of cubes.
___
## Example File

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

