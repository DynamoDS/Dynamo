## 详细
“Extend With Arc”将在输入复合线的起点或终点处添加圆弧，并返回一条组合的复合线。半径输入将确定圆的半径，而长度输入将确定圆弧沿圆的距离。总长度必须小于或等于具有给定半径的完整圆的长度。生成的圆弧将与输入复合线的终点相切。“endOrStart”的布尔输入控制将在复合线的哪一端创建圆弧。值“true”将导致在复合线的终点处创建圆弧，而“false”将在复合线的起点处创建圆弧。在下例中，我们先使用一组随机点和“PolyCurve By Points”来生成一条复合线。然后，我们使用两个“数字”滑块和一个布尔切换来设置“ExtendWithArc”的参数。
___
## 示例文件

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

