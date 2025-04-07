## Description approfondie
`Curve Mapper` node redistributes a series of input values within a defined range and leverages mathematical curves to map them along a specified curve. Mapping, in this context, means the values are redistributed in a way that their x-coordinates follow the shape of the curve along the y-axis.  This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Define the limits for the x-coordinates by setting the minimum and maximum values. These limits set the boundaries within which the points will be redistributed. You can provide either a single count to generate a series of evenly distributed values or an existing series of values, which will be distributed along the x direction within the specified range and then mapped to the curve.

Select a mathematical curve from the provided options, which includes Linear, Sine, Cosine, Perlin Noise, Bezier, Gaussian, Parabolic, Square Root, and Power curves. Use interactive control points to adjust the shape of the selected curve, tailoring it to your specific needs.

You can lock the curve shape using the lock button, preventing further modifications to the curve. Additionally, you can reset the shape to its default state by using the reset button inside the node. If you get NaN or Null as outputs, you can find more details [here](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues) on why you might be seeing this.

For example, to redistribute 80 points along a sine curve within the range of 0 to 20, set Min to 0, Max to 20, and Values to 80. After selecting the sine curve and adjusting its shape as needed, the `Curve Mapper` node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis.

To map unevenly distributed values along a Gaussian curve, set the minimum and maximum range and provide the series of values. After selecting the Gaussian curve and adjusting its shape as needed, the `Curve Mapper` node redistributes the series of values along x-coordinates using the specified range and maps the values along the curve pattern. For in-depth documentation on how the node works and how to set inputs, check out [this blog post](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) focusing on the Curve Mapper.




___
## Exemple de fichier

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
