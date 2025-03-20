## 详细
“TSplineInitialSymmetry.ByAxial”定义 T-Spline 几何图形是否沿所选轴 (x, y, z) 对称。对称可以发生在一个轴、两个轴或全部三个轴上。在创建 T-Spline 几何图形时建立对称后，对称将影响所有后续操作和更改。

在下面的示例中，“TSplineSurface.ByBoxCorners”用于创建 T-Spline 曲面。在此节点的输入中，“TSplineInitialSymmetry.ByAxial”用于定义曲面中的初始对称。然后，“TSplineTopology.RegularFaces”和“TSplineSurface.ExtrudeFaces”分别用于选择和拉伸 T-Spline 曲面的面。然后，拉伸操作将围绕使用“TSplineInitialSymmetry.ByAxial”节点定义的对称轴进行镜像。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
