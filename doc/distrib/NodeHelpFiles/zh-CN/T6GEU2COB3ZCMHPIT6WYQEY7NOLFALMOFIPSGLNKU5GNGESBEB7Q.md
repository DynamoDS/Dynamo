<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## 详细
`NurbsCurve.ByControlPointsWeightsKnots` 允许我们手动控制 NurbsCurve 的权重和节点。权重列表的长度应与控制点列表的长度相同。节点列表的大小必须等于控制点数加上阶数加 1。

在下面的示例中，我们先通过在一系列随机点之间插值来创建 NurbsCurve。我们使用节点、权重和控制点来查找该曲线的相应部分。我们可以使用 `List.ReplaceItemAtIndex` 来修改权重列表。最后，我们使用 `NurbsCurve.ByControlPointsWeightsKnots` 来使用修改后的权重重新创建 NurbsCurve。

___
## 示例文件

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

