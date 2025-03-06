<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
`TSplineReflection.SegmentAngle` 返回每对径向反射分段之间的角度。如果 TSplineReflection 的类型为“轴向”，则该节点返回 0。

在下面的示例中，使用添加的反射创建 T-Spline 曲面。稍后，在图形中，使用 `TSplineSurface.Reflections` 节点查询曲面。然后，结果(反射)用作 `TSplineReflection.SegmentAngle` 的输入，以返回径向反射分段之间的角度。

## 示例文件

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
