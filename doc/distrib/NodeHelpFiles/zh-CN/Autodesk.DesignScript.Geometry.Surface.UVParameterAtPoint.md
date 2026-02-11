## 详细
“UV Parameter At Point”将查找曲面上输入点处曲面的 UV 位置。如果输入点不在曲面上，则该节点将查找曲面上最靠近输入点的点。在下例中，我们先使用“BySweep2Rails”创建一个曲面。然后，我们使用“Code Block”指定用于查找其所在位置处 UN 参数的点。该点不在曲面上，因此该节点使用曲面上最靠近的点作为查找其 UV 参数的位置。
___
## 示例文件

![UVParameterAtPoint](./Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint_img.jpg)

