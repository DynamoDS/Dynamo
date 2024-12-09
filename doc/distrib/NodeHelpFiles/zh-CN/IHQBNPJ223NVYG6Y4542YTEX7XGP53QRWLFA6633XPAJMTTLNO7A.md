<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## 详细
`Surface.TrimWithEdgeLoops` 在指定公差内使用一个或多个闭合 PolyCurves (必须全部都位于曲面上)集修剪曲面。如果需要从输入曲面修剪一个或多个孔，则必须为曲面的边界指定一个外环，为每个孔指定一个内环。如果曲面边界和孔之间的区域需要修剪，则应仅为每个孔提供环。对于没有外环的周期性曲面(如球面)，可以通过反转闭合曲线的方向来控制修剪区域。

公差是在决定曲线末端是否重合以及曲线和曲面是否重合时使用的公差。提供的公差不能小于创建输入 PolyCurve 时使用的任何公差。默认值 0.0 表示将使用在创建输入 PolyCurve 时使用的最大公差。

在下面的示例中，从曲面中修剪出两个环，以返回两个以蓝色亮显的新曲面。数字滑块调整新曲面的形状。

___
## 示例文件

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
