## 详细
“ByColorsAndParameters”将基于输入颜色列表和指定 UV 参数(范围 0 到 1)的相应列表，来创建二维颜色范围。在下例中，我们使用一个代码块创建三种不同的颜色(在本例中仅为绿色、红色和蓝色)，并将它们组合成一个列表。我们使用一个单独的代码块来创建三个 UV 参数，每种颜色一个。这两个列表用作“ByColorsAndParameters”节点的输入。我们使用后续的“GetColorAtParameter”节点以及“Display.ByGeometryColor”节点，来显示一组立方体中的二维颜色范围。
___
## 示例文件

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

