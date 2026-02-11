<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## 详细
`Surface.Thicken (surface, thickness, both_sides)` 通过根据 `thickness` 输入偏移曲面并对端口封口来闭合实体，以创建一个实体。此节点有额外输入来指定是否在两侧加厚。`both_sides` 输入接收布尔值: True 表示在两侧加厚，False 表示在一侧加厚。请注意，`thickness` 参数确定最终实体的总厚度；因此如果 `both_sides` 设置为 True，则结果将是在两侧与原始曲面偏移输入厚度的一半。

在下面的示例中，我们先使用 `Surface.BySweep2Rails` 创建一个曲面。然后，我们通过使用一个数字滑块来确定 `Surface.Thicken` 节点的 `thickness` 输入，以创建一个实体。布尔开关控制是在两侧加厚还是仅在一侧加厚。

___
## 示例文件

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
