<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
顶点的功能边价超出了相邻边的简单计数，并考虑了影响顶点在其周围区域中混合的虚拟栅格线。它提供了对顶点及其边在变形和细化操作过程中如何影响曲面的更细致了解。
当用于常规顶点和 T 点时，`TSplineVertex.FunctionalValence` 节点返回值 "4"，这意味着曲面由栅格形状中的样条线引导。"4" 以外的任何功能边价意味着顶点是星形点，并且围绕顶点的混合将不太平滑。

在下面的示例中，`TSplineVertex.FunctionalValence` 用于 T-Spline 平面曲面的两个 T 点顶点。`TSplineVertex.Valence` 节点返回值 3，而选定顶点的功能边价为 4，这是 T 点的特定值。`TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 节点用于可视化要分析的顶点的位置。

## 示例文件

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
