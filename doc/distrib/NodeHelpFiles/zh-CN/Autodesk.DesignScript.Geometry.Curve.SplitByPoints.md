## 详细
“Split By Points”将在指定点处分割输入曲线，并返回生成的分段列表。如果指定的点不在曲线上，则该节点会查找曲线上最接近输入点的点，并在这些结果点处分割曲线。在下例中，我们先使用“ByPoints”节点创建一条 Nurbs 曲线，其中一组随机生成的点作为输入。一组相同的点用作“SplitByPoints”节点中的点列表。结果是生成的点之间的曲线段列表。
___
## 示例文件

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

