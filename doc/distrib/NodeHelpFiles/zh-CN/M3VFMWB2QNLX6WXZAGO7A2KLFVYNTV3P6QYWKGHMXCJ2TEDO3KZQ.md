## In-Depth
`TSplineSurface.BuildPipes` 使用曲线网络生成 T-Spline 管道曲面。如果各个管道的端点在 `snappingTolerance` 输入设置的最大公差范围内，则这些管道视为已连接。如果输入的列表长度等于管道数，则此节点的结果可以使用一组输入进行微调(可以为所有管道设置值，也可以单独设置)。以下输入可以通过这种方式使用: `segmentsCount`、`startRotations`、`endRotations`、`startRadii`、`endRadii`、`startPositions` 和 `endPositions`。

在下面的示例中，在端点处连接的三条曲线作为 `TSplineSurface.BuildPipes` 节点的输入提供。在本例中，`defaultRadius` 是所有三个管道的单个值，以默认定义管道的半径(除非提供开始半径和结束半径)。
接下来，`segmentsCount` 为每个单独的管道设置三个不同的值。输入是三个值的列表，每个列表对应一个管道。

如果将 `autoHandleStart` 和 `autoHandleEnd` 设置为 False，则可以进行更多调整。这允许控制每个管道的开始和结束旋转(`startRotations` 和 `endRotations` 输入)，以及通过指定 `startRadii` 和 `endRadii` 来控制每个管道终点和起点处的半径。最后，`startPositions` 和 `endPositions` 允许分别在每条曲线的起点或终点处偏移线段。此输入需要一个与线段开始或结束处的曲线参数相对应的值(值在 0 和 1 之间)。

## 示例文件
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
