## 详细
闭合曲面是一个形成没有洞口或边界的完整形状的曲面。
在下面的示例中，使用 `TSplineSurface.IsClosed` 检查通过 `TSplineSurface.BySphereCenterPointRadius` 生成的 T-Spline 球体，以检查其是否是开放的，这会返回负结果。这是因为 T-Spline 球体尽管看起来是闭合的，但实际上在多个边和顶点堆叠在一个点中的极点处是开放的。

然后，使用 `TSplineSurface.FillHole` 节点填充 T-Spline 球体中的间隙，这会导致曲面填充处发生轻微变形。当通过 `TSplineSurface.IsClosed` 节点再次检查它时，现在会产生一个正结果，这意味着它是闭合的。
___
## 示例文件

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
