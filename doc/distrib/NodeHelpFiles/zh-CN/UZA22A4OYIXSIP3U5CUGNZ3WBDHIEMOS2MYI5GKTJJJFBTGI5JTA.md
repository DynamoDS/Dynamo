<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## 详细
在下面的示例中，使用 `TSplineSurface.ByCombinedTSplineSurfaces` 节点将 T-Spline 曲面的两半连接为一个曲面。沿镜像平面的顶点重叠，当使用 `TSplineSurface.MoveVertices` 节点移动其中一个顶点时，这些顶点变为可见。要修复此问题，请使用 `TSplineSurface.WeldCoincidentVertices` 节点执行接合。移动顶点的结果现在不同了，平移到一侧以更好地预览。
___
## 示例文件

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
