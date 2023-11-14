## In Depth
`TSplineSurface.Thicken(vector, softEdges)` thickens a T-Spline surface guided by the specified vector. The thickening operation duplicates the surface in the `vector` direction and then connects the two surfaces by joining their edges. The `softEdges` Boolean input controls whether the resulting edges are smoothed (true) or creased (false).

In the example below, a T-Spline cylindric surface is thickened using the `TSplineSurface.Thicken(vector, softEdges)` node. The resulting surface is translated to the side for a better visualization.


___
## Example File

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)