## In Depth

XY Line Plot creates a chart with one or more lines plotted by their x- and y-values. Label your lines or change the number of lines by entering a list of string labels in the labels input. Each label creates a new color-coded line. If you input only one string value, only one line will be created.

To determine the placement of each point along each line, use a list of lists containing double values for the x- and y-values inputs. There must be an equal number of values in the x-values and y-values inputs. The number of sublists must also match the number of string values in the labels input.
For example, if you want to create 3 lines, each with 5 points, provide a list with 3 string values in the labels input to name each line, and provide 3 sublists with 5 double values in each for both the x- and y-values.

To assign a color for each line, insert a list of colors in the colors input. When assigning custom colors, the number of colors must match the number of string values in the labels input. If no colors are assigned, random colors will be used.

___
## Example File

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

