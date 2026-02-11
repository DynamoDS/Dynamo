## 详细
“Mesh.Sphere”创建一个网格球体，以输入的“origin”点为中心，具有给定的“radius”和许多“divisions”。“icosphere”布尔输入用于在“icosphere”和“UV-Sphere”球形网格类型之间切换。与 UV 网格相比，icosphere 网格使用更多的规则三角形覆盖球体，并且往往会在后续建模操作中提供更好的结果。UV 网格的两极与球体轴对齐，并且三角形层绕轴纵向生成。

在使用 icosphere 的情况下，围绕球轴的三角形数量可以低至指定的分割数，最多可以是该数的两倍。“UV 球体”的分割决定了围绕球体纵向生成的三角形层的数量。当“divisions”输入设置为零时，该节点将返回一个 UV 球体，其中任一网格类型的默认分割数为 32。

在下面的示例中，“Mesh.Sphere”节点用于创建两个半径和分割相同但使用不同方法的球体。当“icosphere”输入设置为“True”时，“Mesh.Sphere”返回“icosphere”。或者，当“icosphere”输入设置为“False”时，“Mesh.Sphere”节点返回“UV-sphere”。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
