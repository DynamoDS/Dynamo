<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## 详细
长方体模式和平滑模式是查看 T-Spline 曲面的两种方式。平滑模式是 T-Spline 曲面的真实形状，对于预览模型的美观性和尺寸非常有用。另一方面，长方体模式可以将见解投射到曲面结构上并更好地了解它，同时也是预览大型或复杂几何图形的更快选项。`TSplineSurface.EnableSmoothMode` 节点允许在几何图形开发的各个阶段在这两个预览状态之间切换。

在下面的示例中，对 T-Spline 长方体曲面执行“倒角”操作。结果是先在长方体模式(长方体曲面的 `inSmoothMode` 输入设置为 false)下进行可视化，以便更好地了解形状的结构。然后，通过 `TSplineSurface.EnableSmoothMode` 节点激活平滑模式，结果平移到右侧以同时预览这两种模式。
___
## 示例文件

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
