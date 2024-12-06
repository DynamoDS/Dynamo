## In Depth
`TSplineSurface.Thicken(distance, softEdges)` thickens a T-Spline surface outwardly (or inwardly, when a negative `distance` value is provided) by a specified `distance` along its face normals. The `softEdges` Boolean input controls whether the resulting edges are smoothed (true) or creased (false).

In the example below, a T-Spline cylindric surface is thickened using the `TSplineSurface.Thicken(distance, softEdges)` node. The resulting surface is translated to the side for a better visualization.
___
## Example File

![TSplineSurface.Thicken](./UHLOMXPCNY3C36FQ45G3HQGKIZLSUE2QX4N7FY7ZCCOEN7F7Q6YA_img.jpg)