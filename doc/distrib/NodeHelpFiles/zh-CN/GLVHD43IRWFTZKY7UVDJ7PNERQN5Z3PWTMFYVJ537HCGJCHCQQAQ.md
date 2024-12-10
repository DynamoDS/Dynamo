<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
`TSplineReflection.SegmentsCount` 返回径向反射的分段数。如果 TSplineReflection 的类型为“轴向”，则该节点返回值 0。

在下面的示例中，使用添加的反射创建 T-Spline 曲面。稍后，在图形中，使用 `TSplineSurface.Reflections` 节点查询曲面。然后，结果(反射)用作 `TSplineReflection.SegmentsCount` 的输入，以返回用于创建 T-Spline 曲面的径向反射的分段数。

## 示例文件

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
