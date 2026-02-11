<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## 详细
`PolyCurve.ByGroupedCurves` 通过对多条连接的曲线进行分组来创建新的 PolyCurve。它根据曲线与其他曲线的接近度(接触或在给定的连接公差内)对曲线进行分组，以将它们连接到单个 PolyCurve。

在下面的示例中，对五边形进行分解，并随机化其曲线。然后，使用 `PolyCurve.ByGroupedCurves` 以将这些曲线分组到 PolyCurve 中。
___
## 示例文件

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
