## In-Depth
`TSplineReflection.ByRadial` 返回一个可用作 `TSplineSurface.AddReflections` 节点输入的对象。该节点将一个平面用作输入，该平面的法线作为旋转几何图形的轴。与 TSplineInitialSymmetry 非常类似，TSplineReflection 在创建 TSplineSurface 时建立后，就会影响所有后续操作和更改。

在下面的示例中，`TSplineReflection.ByRadial` 用于定义 T-Spline 曲面的反射。`segmentsCount` 和 `segmentAngle` 输入用于控制围绕给定平面的法线反射几何图形的方式。然后，该节点的输出用作 `TSplineSurface.AddReflections` 节点的输入以创建一个新的 T-Spline 曲面。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
