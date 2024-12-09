## 详细
`PolyCurve.Heal` 接收自相交 PolyCurve 并返回不自相交的新 PolyCurve。输入 PolyCurve 的自相交点不能超过 3 个。换句话说，如果 PolyCurve 的任何一条线段与其他 2 条以上线段相交，则修复将不起作用。输入大于 0 的 `trimLength`，然后长度超过 `trimLength` 的结束段将不会被修剪。

在下面的示例中，使用 `PolyCurve.Heal` 修复自相交的 PolyCurve。
___
## 示例文件

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
