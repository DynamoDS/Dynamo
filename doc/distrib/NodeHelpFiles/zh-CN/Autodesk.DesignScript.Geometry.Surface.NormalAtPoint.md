## 详细
“Normal At Point”将查找曲面上输入点处曲面的法线向量。如果输入点不在曲面上，则该节点会查找曲面上最靠近输入点的点。在下例中，我们先使用“BySweep2Rails”创建一个曲面。然后，我们使用“Code Block”指定用于查找所在位置处法线的点。该点不在曲面上，因此该节点使用曲面上最靠近的点作为用于查找所在位置处法线的位置。
___
## 示例文件

![NormalAtPoint](./Autodesk.DesignScript.Geometry.Surface.NormalAtPoint_img.jpg)

