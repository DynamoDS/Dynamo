## 详细
长方体模式和平滑模式是查看 T-Spline 曲面的两种方式。平滑模式是 T-Spline 曲面的真实形状，对于预览模型的美观性和尺寸非常有用。另一方面，长方体模式可以将见解投射到曲面结构上并更好地了解它，同时也是预览大型或复杂几何图形的更快选项。长方体模式和平滑模式可以在创建初始 T-Spline 曲面时进行控制，也可以稍后使用 `TSplineSurface.EnableSmoothMode` 等节点进行控制。

如果 T-Spline 变为无效，其预览将自动切换到长方体模式。`TSplineSurface.IsInBoxMode` 节点是识别曲面是否变为无效的另一种方法。

在下面的示例中，在将 `smoothMode` 输入设置为 true 的情况下创建 T-Spline 平面曲面。删除它的两个面，从而使该曲面无效。曲面预览切换为长方体模式，尽管无法单独从预览中得知。`TSplineSurface.IsInBoxMode` 节点用于确认曲面是否处于长方体模式下。
___
## 示例文件

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
