## 详细

在下面的示例中，使用
`TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)` 节点将 T-Spline 曲面与 NURBS 曲线匹配。
该节点所需的最小输入是基础 `tSplineSurface`、在 `tsEdges` 输入中提供的一组曲面边以及曲线或
曲线列表。
以下输入控制匹配的参数:
- `continuity` 允许设置匹配的连续性类型。输入需要值 0、1 或 2，对应于 G0 位置、G1 切线和 G2 曲率连续性。但是，如要将曲面与曲线匹配，则仅 G0 (输入值为 0)可用。
- `useArcLength` 控制路线类型选项。如果设置为 True，则使用的路线类型是“弧长”。
此路线最大程度地减少 T-Spline 曲面的每个点和曲线上相应点之间的物理距离。
当提供 False 输入时，路线类型是“参数化”-
T-Spline 曲面上的每个点都与沿匹配目标曲线的可比参数化距离的点
匹配。
- `useRefinement`，当设置为 True 时，将控制点添加到曲面以尝试
在给定的 `refinementTolerance` 内匹配目标
- `numRefinementSteps` 是在尝试达到 `refinementTolerance` 时对基础 T-Spline 曲面进行细分的最大次数。
如果 `useRefinement` 设置为 False，将忽略 `numRefinementSteps` 和 `refinementTolerance`。
- `usePropagation` 控制曲面受匹配影响的程度。当设置为 False 时，曲面受影响最小。当设置为 True 时，曲面在提供的 `widthOfPropagation` 距离内受影响。
- `scale` 是切线比例，影响 G1 和 G2 连续性的结果。
- `flipSourceTargetAlignment` 反转路线方向。


## 示例文件

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
