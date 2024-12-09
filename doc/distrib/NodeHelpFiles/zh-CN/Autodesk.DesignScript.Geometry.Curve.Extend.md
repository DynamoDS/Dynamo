## 详细
“Extend”将按给定输入距离延伸输入曲线。“pickSide”输入将曲线的起点或终点用作输入，并确定要延伸的曲线端点。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。我们使用查询节点“Curve.EndPoint”来查找曲线的终点，以用作“pickSide”输入。一个“数字”滑块允许我们控制延伸的距离。
___
## 示例文件

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

