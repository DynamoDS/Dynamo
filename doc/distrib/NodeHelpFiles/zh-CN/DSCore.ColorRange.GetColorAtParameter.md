## 详细
“GetColorAtParameter”将获取输入的二维颜色范围，并返回一个指定 UV 参数(范围 0 到 1)处的颜色列表。在下例中，我们先使用“ByColorsAndParameters”节点以及一个颜色列表和用于设置范围的参数列表，来创建一个二维颜色范围。一个代码块用于生成介于 0 和 1 之间的数字范围，它用作“UV.ByCoordinates”节点中的 u 和 v 输入。此节点的连缀设置为叉积。一组立方体以类似方式创建，连缀为叉积的“Point.ByCoordinates”节点使用它来创建立方体数组。然后，我们将“Display.ByGeometryColor”节点与立方体数组以及从“GetColorAtParameter”节点获取的颜色列表一起使用。
___
## 示例文件

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

