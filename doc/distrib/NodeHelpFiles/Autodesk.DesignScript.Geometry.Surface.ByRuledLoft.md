## In Depth
Surface by Ruled Loft takes an ordered list of curves as an input and lofts a straigth-line ruled surface between the curves. Compared to ByLoft, ByRuledLoft can be slightly faster, but the resulting surface is less smooth. In the example below, we start with a line along the X-axis. We translate this line into a series of lines that follow a sine curve in the y-direction. Using this resulting list of lines as the input for a Surface ByRuledLoft results in a surface with straight-line segments between the input curves.
___
## Example File

![ByRuledLoft](./Autodesk.DesignScript.Geometry.Surface.ByRuledLoft_img.jpg)

