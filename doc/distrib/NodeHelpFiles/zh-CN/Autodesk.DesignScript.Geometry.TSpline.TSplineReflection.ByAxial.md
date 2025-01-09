## In-Depth
`TSplineReflection.ByAxial` 返回 `TSplineReflection` 对象，该对象可用作 `TSplineSurface.AddReflections` 节点的输入。
`TSplineReflection.ByAxial` 节点的输入是一个将用作镜像平面的平面。与 TSplineInitialSymmetry 非常类似，TSplineReflection 在为 TSplineSurface 建立后，就会影响所有后续操作和更改。

在下面的示例中，`TSplineReflection.ByAxial` 用于创建位于 T-Spline 圆锥体顶部的 TSplineReflection。然后，该反射用作 `TSplineSurface.AddReflections` 节点的输入，以反射圆锥体并返回一个新的 T-Spline 曲面。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
