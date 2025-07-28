<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` 定义 T-Spline 几何图形是否具有径向对称。仅可为允许径向对称的 T-Spline 基本体(圆锥体、球体、旋转、圆环体)引入径向对称。在创建 T-Spline 几何图形时建立径向对称后，径向对称将影响所有后续操作和更改。

需要定义所需数量的 `symmetricFaces` 以应用对称，其中 1 是最小值。无论 T-Spline 曲面必须从多少半径和高度跨度开始，它都将进一步分割为所选数量的 `symmetricFaces`。

在下面的示例中，创建 `TSplineSurface.ByConePointsRadii`，并通过使用 `TSplineInitialSymmetry.ByRadial` 节点应用径向对称。然后，`TSplineTopology.RegularFaces` 和 `TSplineSurface.ExtrudeFaces` 节点分别用于选择和拉伸 T-Spline 曲面的面。拉伸对称应用，对称面数的滑块演示如何细分径向跨度。

## 示例文件

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
