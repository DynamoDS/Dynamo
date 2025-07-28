## 详细
`TSplineSurface.Standardize` 节点用于标准化 T-Spline 曲面。
标准化意味着为 NURBS 转换准备一个 T-Spline 曲面，并意味着延伸所有 T 点，直到它们与星形点至少相隔两条等参曲线。标准化不会改变曲面的形状，但可能会添加控制点以满足使曲面与 NURBS 兼容所需的几何图形要求。

在下面的示例中，通过 `TSplineSurface.ByBoxLengths` 生成的 T-Spline 曲面将它的一个面进行细分。
`TSplineSurface.IsStandard` 节点用于检查曲面是否是标准曲面，但会产生负结果。
然后，使用 `TSplineSurface.Standardize` 标准化曲面。使用 `TSplineSurface.IsStandard` 检查生成的曲面，这确认它现在是标准曲面。
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## 示例文件

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
