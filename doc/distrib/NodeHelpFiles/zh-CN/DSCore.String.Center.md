## 详细
“Polygon Center”将通过获取角点的平均值来查找给定多边形的中心。对于凹多边形，中心可能实际上位于多边形之外。在下例中，我们先生成随机角度和半径的列表，以用作“Point By Cylindrical Coordinates”的输入。通过先对角度进行排序，我们确保生成的多边形将按角度增加的顺序进行连接，因此不会自相交。然后，我们可以使用“Center”来获取点的平均值并查找多边形中心。
___
## 示例文件

![Center](./DSCore.String.Center_img.jpg)

