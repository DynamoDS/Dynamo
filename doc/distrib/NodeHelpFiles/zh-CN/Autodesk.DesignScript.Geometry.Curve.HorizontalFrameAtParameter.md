## 详细
“Horizontal Frame At Parameter”将返回一个与指定参数处的输入曲线对齐的坐标系。曲线的参数化在 0 到 1 的范围内进行测量，其中 0 表示曲线的开始，1 表示曲线的结束。生成的坐标系的 Z 轴将位于世界 Z 方向，Y 轴位于指定参数处的曲线切线方向。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。一个设置为范围 0 到 1 的“数字”滑块用于控制“HorizontalFrameAtParameter”节点的参数输入。
___
## 示例文件

![HorizontalFrameAtParameter](./Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter_img.jpg)

