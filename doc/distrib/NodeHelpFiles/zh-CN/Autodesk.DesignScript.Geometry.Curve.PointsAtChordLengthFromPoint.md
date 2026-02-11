## 详细
“Points At Chord Length From Point”将返回曲线上的点列表，该点列表根据输入弦长从曲线上的指定点开始依次测量。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。“PointAtParameter”节点与设置为范围 0 到 1 的“数字”滑块一起用于为“PointsAtChordLengthFromPoint”节点确定曲线上的初始点。最后，使用第二个“数字”滑块来调整要使用的直线弦长。
___
## 示例文件

![PointsAtChordLengthFromPoint](./Autodesk.DesignScript.Geometry.Curve.PointsAtChordLengthFromPoint_img.jpg)

