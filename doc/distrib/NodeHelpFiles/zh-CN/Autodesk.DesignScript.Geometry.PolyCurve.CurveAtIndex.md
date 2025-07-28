## 详细
“Curve At Index”将返回给定复合线的输入索引处的曲线段。如果复合线中的曲线数小于给定索引，“CurveAtIndex”将返回空值。“endOrStart”输入接受布尔值“true”或“false”。如果为“true”，“CurveAtIndex”将从复合线的第一段开始计数。如果为“false”，它将从最后一段开始倒数。在下例中，我们生成一组随机点，然后使用“PolyCurve By Points”创建一条开放的复合线。接着，我们可以使用“CurveAtIndex”从该复合线中提取特定分段。
___
## 示例文件

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

