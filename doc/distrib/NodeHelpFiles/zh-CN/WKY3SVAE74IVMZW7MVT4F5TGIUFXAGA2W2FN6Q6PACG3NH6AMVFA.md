<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## 详细
在下面的示例中，通过 `TSplineSurface.ByBoxLengths` 节点生成一个 T-Spline 曲面。
使用 `TSplineTopology.FaceByIndex` 节点选择一个面，然后使用 `TSplineSurface.SubdivideFaces` 节点对该面进行细分。
此节点将指定的面分割为较小面 - 四个面用于常规面，三个、五个或更多面用于多边形。
当 `exact` 的布尔输入设置为 true 时，结果是在添加细分时尝试保持与原始曲面完全相同形状的曲面。可以添加更多等参曲线以保留形状。如果设置为 false，则节点仅细分一个选定面，这通常会生成与原始曲面不同的曲面。
`TSplineFace.UVNFrame` 和 `TSplineUVNFrame.Position` 节点用于亮显要细分的面的中心。
___
## 示例文件

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
