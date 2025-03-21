## 詳細
The `Curve Mapper` node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Define the limits for both x and y coordinates by setting the minimum and maximum values. These limits set the boundaries within which the points will be redistributed. Next, select a mathematical curve from the provided options, which includes Linear, Sine, Cosine, Perlin Noise, Bezier, Gaussian, Parabolic, Square Root, and Power curves. Use the interactive control points to adjust the shape of the selected curve, tailoring it to your specific needs.

You can lock the curve shape using the lock button, which prevents further modifications to the curve. Additionally, you can reset the shape to its default state by using the reset button inside the node.

Specify the number of points to be redistributed by setting the Count input. The node calculates new x-coordinates for the specified number of points based on the selected curve and defined limits. The points are redistributed in a way that their x-coordinates follow the shape of the curve along the y-axis.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the `Curve Mapper` node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.




___
## サンプル ファイル

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
