<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## 详细
“Curve.SweepAsSurface”将通过沿指定路径扫掠输入曲线来创建曲面。在下面的示例中，我们使用“代码块”创建“Arc.ByThreePoints”节点的三个点，从而创建一条要扫掠的曲线。路径曲线创建为沿 X 轴的一条简单直线。“Curve.SweepAsSurface”沿路径曲线移动轮廓曲线，以创建曲面。“cutEndOff”参数是一个布尔值，用于控制扫掠曲面的端部处理方式。当设置为“true”时，将垂直(法线)路径曲线切割曲面的末端，从而生成干净、平坦的端点。当设置为"false"(默认值)时，曲面末端遵循轮廓曲线的自然形状，而不进行任何修剪，这可能会根据路径曲率的不同产生有角度或不均匀的端点。
___
## 示例文件

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

