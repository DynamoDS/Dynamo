## 详细
“Points At Segment Length From Point”将返回曲线上的点列表，该点列表根据输入段长度从曲线上的指定点开始依次测量。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。“PointAtParameter”节点与设置为范围 0 到 1 的“数字”滑块一起用于为“PointsAtSegmentLengthFromPoint”节点确定曲线上的初始点。最后，使用第二个“数字”滑块来调整要使用的曲线段长度。
___
## 示例文件

![PointsAtSegmentLengthFromPoint](./Autodesk.DesignScript.Geometry.Curve.PointsAtSegmentLengthFromPoint_img.jpg)

