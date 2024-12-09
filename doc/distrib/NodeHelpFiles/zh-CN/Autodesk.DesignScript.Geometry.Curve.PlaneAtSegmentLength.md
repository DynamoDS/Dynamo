## 详细
“Plane At Segment Length”将返回一个与某点处的曲线对齐的平面，该点位于曲线上从起点开始测量的指定距离处。如果输入长度大于曲线的总长度，则此节点将使用曲线的终点。生成平面的法线向量将对应于曲线的切线。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。一个“数字”滑块用于控制“PlaneAtSegmentLength”节点的参数输入。
___
## 示例文件

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

