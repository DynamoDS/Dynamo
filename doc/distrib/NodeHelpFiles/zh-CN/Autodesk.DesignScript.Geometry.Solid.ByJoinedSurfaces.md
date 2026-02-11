## 详细
“Solid by Joined Surfaces”会将曲面列表作为输入，并将返回由曲面定义的单个实体。这些曲面必须定义一个闭合曲面。在下例中，我们先将一个圆作为基础几何图形。对该圆进行修补以创建一个曲面，然后沿 Z 方向平移该曲面。然后，我们拉伸该圆以生成边。“List.Create”用于创建一个由底面、侧面和顶面组成的列表，然后我们使用“ByJoinedSurfaces”将该列表转换为单个闭合实体。
___
## 示例文件

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

