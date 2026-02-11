<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## 详细
`TSplineSurface.DuplicateFaces` 节点创建一个仅由选定复制面组成的新 T-Spline 曲面。

在下面的示例中，通过 `TSplineSurface.ByRevolve` (使用 NURBS 曲线作为轮廓)创建 T-Spline 曲面。
然后，使用 `TSplineTopology.FaceByIndex` 选择曲面上的一组面。使用 `TSplineSurface.DuplicateFaces` 复制这些面，然后将生成的曲面移动到一侧以更好地可视化。
___
## 示例文件

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
