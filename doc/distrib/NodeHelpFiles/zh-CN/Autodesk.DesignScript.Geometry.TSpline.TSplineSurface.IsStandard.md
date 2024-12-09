## 详细
当所有 T 点与星形点至少相隔两条等参曲线时，T-Spline 曲面是标准曲面。将 T-Spline 曲面转换为 NURBS 曲面需要进行标准化。

在下面的示例中，通过 `TSplineSurface.ByBoxLengths` 生成的 T-Spline 曲面将其一个面进行细分。`TSplineSurface.IsStandard` 用于检查曲面是否是标准曲面，但产生负结果。
然后，使用 `TSplineSurface.Standardize` 标准化曲面。引入新的控制点，而不改变曲面的形状。使用 `TSplineSurface.IsStandard` 检查生成的曲面，这确认它现在是否是标准曲面。
`TSplineFace.UVNFrame` 和 `TSplineUVNFrame.Position` 节点用于亮显曲面中的细分面。
___
## 示例文件

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
