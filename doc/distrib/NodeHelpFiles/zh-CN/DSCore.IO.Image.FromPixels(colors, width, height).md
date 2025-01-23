## 详细
具有宽度和高度的“From Pixels”将基于输入的平面颜色列表创建一个图像，其中每种颜色都将成为一个像素。宽度和高度的乘积应等于颜色总数。在下例中，我们先使用“ByARGB”节点创建颜色列表。一个代码块创建从 0 到 255 的值范围，当其连接到 r 和 g 输入时，会生成从黑色到黄色的一系列颜色。我们创建一个宽度为 8 的图像。“Count”节点和“Division”节点用于确定图像的高度。“Watch Image”节点可用于预览创建的图像。
___
## 示例文件

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

