## 详细
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

通过设置最小值和最大值来定义 x 和 y 坐标的限制。这些限制设置了重新分配点的边界。接下来，从提供的选项中选择数学曲线，包括“线性”、“正弦”、“余弦”、“柏林噪波”、“贝塞尔曲线”、“高斯曲线”、“抛物线”、“平方根”和“幂曲线”。使用交互式控制点调整所选曲线的形状，从而根据您的特定需求进行定制。

可以使用锁定按钮锁定曲线形状，这样可以防止对曲线进行进一步修改。此外，还可以使用节点内的重置按钮将形状重置为其默认状态。

通过设置 Count 输入，指定要重新分布的点数。该节点根据选定曲线和定义的限制计算指定数量的点的新 x 坐标。点的重新分布方式是使其 x 坐标沿 y 轴跟随曲线的形状。

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## 示例文件


