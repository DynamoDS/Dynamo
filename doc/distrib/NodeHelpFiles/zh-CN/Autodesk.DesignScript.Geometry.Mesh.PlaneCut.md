## 详细
“Mesh.PlaneCut”返回已被给定平面剪切的网格。剪切的结果是位于平面侧面、“plane”输入的法线方向上的网格部分。“makeSolid”参数控制网格是否被视为“Solid”，在这种情况下，剪切会用最少的三角形填充以覆盖每个孔。

在下面的示例中，通过“Mesh.BooleanDifference”操作获取的空心网格被平面以一定角度剪切。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
