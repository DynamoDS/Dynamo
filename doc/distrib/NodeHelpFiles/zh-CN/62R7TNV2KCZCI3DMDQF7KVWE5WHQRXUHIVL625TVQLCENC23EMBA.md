<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## 详细
`Surface.ToNurbsSurface` 接收一个曲面作为输入并返回接近输入曲面的 NurbsSurface。`limitSurface` 输入确定在转换之前是否应将曲面恢复到其原始参数范围，例如，当曲面的参数范围受到限制时是在“修剪”操作之后。

在下面的示例中，我们使用 `Surface.ByPatch` 节点(其中闭合 NurbsCurve 作为输入)创建一个曲面。请注意，当我们使用此曲面作为 `Surface.ToNurbsSurface` 节点的输入时，结果是具有四条边的未修剪 NurbsSurface。


___
## 示例文件

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
