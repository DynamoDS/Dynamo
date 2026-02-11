<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## 详细
创建具有指定控制顶点、节点、权重和 U V 阶数的 NurbsSurface。对数据有若干限制，如果数据被破坏，将导致该函数运行失败并引发异常。阶数: U 和 V 阶数应 >= 1 (分段线性样条曲线)且小于 26 (ASM 支持的最大 B 样条曲线基础阶数)。权重: 所有权重值(若提供)应仅限于正值。权重小于 1e-11 将被拒绝，并且该函数会运行失败。节点: 两个节点向量应为非递减序列。内部节点多重性不应大于起点/终点节点处的阶数加 1 和内部节点处的阶数(这样才能表示具有 G1 不连续性的曲面)。请注意，非固定边界节点向量均受支持，但会被转换为固定边界节点，并将相应更改应用于控制点/权重数据。
___
## 示例文件



