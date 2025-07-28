<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## 详细
`Solid.BySweep` 通过沿指定路径扫掠输入的闭合轮廓曲线来创建实体。

在下面的示例中，我们使用一个矩形作为基本轮廓曲线。通过使用余弦函数和一系列角度来改变一组点的 x 坐标，以创建路径。这些点用作 `NurbsCurve.ByPoints` 节点的输入。然后，我们通过沿创建的余弦曲线扫掠矩形来创建实体。
___
## 示例文件

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
