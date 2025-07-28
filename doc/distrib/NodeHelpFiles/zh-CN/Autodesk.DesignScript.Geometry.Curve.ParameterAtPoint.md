## 详细
“Parameter at Point”将返回曲线上指定点的参数值。如果输入点不在曲线上，则“Parameter At Point”将返回曲线上接近输入点的点的参数。在下例中，我们先使用“ByControlPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。使用“Code Block”创建一个额外的点以指定 X 和 Y 坐标。“ParameterAtPoint”节点返回曲线上最接近输入点的点处的参数。
___
## 示例文件

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

