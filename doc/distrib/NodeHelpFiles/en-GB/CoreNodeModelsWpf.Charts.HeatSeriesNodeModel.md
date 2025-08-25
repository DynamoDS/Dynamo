## In Depth

Heat Series Plot creates a chart where data points are represented as rectangles in different colors along a color range.

Assign labels for each column and row by entering a list of string labels into the x-labels and y-labels inputs, respectively. The number of x-labels and y-labels doesn't have to match.

Define a value for each rectangle with the values input. The number of sublists must match the number of string values in the x-labels input, as it represents the number of columns. The values inside each sublist represent the number of rectangles in each column. For example, 4 sublists correspond with 4 columns, and if each sublist has 5 values, the columns have 5 rectangles each.

As another example, to create a grid with 5 rows and 5 columns, provide 5 string values in the x-labels input as well as the y-labels input. The x-label values will appear below the chart along the x-axis, and the y-label values will appear to the left of the chart along the y-axis.

In the values input, enter a list of lists, each sublist containing 5 values. Values are plotted column by column from left to right and bottom to top, so the first value in the first sublist is the bottom rectangle in the left column, the second value is the rectangle above that, and so on. Each sublist represents a column in the plot.

You can assign a color range to differentiate the data points by entering a list of color values in the colors input. The lowest value in the chart will be equal to the first color, and the highest value will be equal to the last color, with other values in between along the gradient. If no color range is assigned, the data points will be given a random color from lightest to darkest shade.

For best results, use one or two colors. The example file provides a classic example of two colors, blue and red. When they are used as color inputs, the Heat Series Plot will automatically create a gradient between these colors, with low values represented in shades of blue and high values in shades of red.

___
## Example File

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

