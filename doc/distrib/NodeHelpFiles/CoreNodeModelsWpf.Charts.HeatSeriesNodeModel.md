## In Depth

Heat Series Plot creates a chart where data points are represented as rectangles in different colors along a color range. Assign labels for each row and column by entering a list of string labels into the x-labels and y-labels inputs, respectively.

Determine the number of rectangles in the chart with the values input. In the list of lists containing double values, each list assigns the given number of rectangles column by column, starting from the left, and bottom to top in each column. For example, 4 lists will create 4 columns, and if each list has 5 values, the columns will have 5 rectangles each.

You can assign a color range to differentiate the data points by entering a list of color values in the colors input. The lowest value in the chart will be equal to the first color, and the highest value will be equal to the last color, with other values in between along the gradient. If no color range is assigned, the data points will be colored randomly from lightest to darkest shade.
___
## Example File

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

