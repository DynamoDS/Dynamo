## 详细
`PolySurface.BySweep (rail, crossSection)` 通过沿轨道扫掠一列不相交的连接线来返回 PolySurface。`crossSection` 输入可以接收一列必须在起点或终点相交的连接曲线，否则该节点不会返回 PolySurface。此节点类似于 `PolySurface.BySweep (rail, profile)`，唯一区别是 `crossSection` 输入接收一列曲线，而 `profile` 仅接收一条曲线。

在下面的示例中，通过沿圆弧扫掠来创建 PolySurface。


___
## 示例文件

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
