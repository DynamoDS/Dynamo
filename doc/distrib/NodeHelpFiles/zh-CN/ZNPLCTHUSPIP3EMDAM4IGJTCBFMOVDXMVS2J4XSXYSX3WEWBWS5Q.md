<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## 详细
“Coordinate System At Segment Length”将返回一个与指定曲线长度(从曲线的起点测量)处的输入曲线对齐的坐标系。生成的坐标系的 X 轴将位于曲线法线方向，Y 轴位于指定长度的曲线切线方向。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。一个“数字”滑块用于控制“CoordinateSystemAtParameter”节点的段长度输入。如果指定长度长于曲线长度，则此节点将在曲线的终点处返回一个坐标系。
___
## 示例文件

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

