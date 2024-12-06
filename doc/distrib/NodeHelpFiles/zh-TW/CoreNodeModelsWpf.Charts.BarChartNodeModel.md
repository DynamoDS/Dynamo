## In Depth

Bar Chart creates a chart with vertically oriented bars. Bars can be organized under multiple groups and labeled with color coding. You have the option to create a single group by entering a single double value, or multiple groups by entering multiple double values per sublist in the values input. To define categories, insert a list of string values in the labels input. Each value creates a new color-coded category.

To assign a value (height) to each bar, enter a list of lists containing double values in the values input. Each sublist will determine the number of bars, and which category they belong to, in the same order as the labels input. If you have a single list of double values, only one category will be created. The number of string values in the labels input must match the number of sublists in the values input.

To assign a color for each category, insert a list of colors in the colors input. When assigning custom colors, the number of colors must match the number of string values in the labels input. If no colors are assigned, random colors will be used.

## Example: Single group

Imagine you want to represent average user ratings for an item over the first three months of the year. To visualize this, you need a list of three string values, labeled January, February, and March.
So, for the labels input, we’ll provide the following list in a Code Block:

[“January”, “February”, “March”];

You can also use String nodes connected to the List Create node to create your list.

Next, in the values input, we’ll enter the average user rating for each of the three months as a list of lists:

[[3.5], [5], [4]];

Note that since we have three labels, we need three sublists.

Now when the graph is run, the bar chart will be created, with each colored bar representing the average customer rating for the month. You can continue using the default colors, or plug in a list of custom colors in the colors input.

## Example: Multiple groups

You can leverage the Bar Chart node’s grouping functionality by entering more values in each sublist in the values input. In this example, let’s create a chart visualizing the number of doors in three variations of three models, Model A, Model B, and Model C.

To do this, we’ll first provide the labels:

[“Model A”, “Model B”, “Model C”];

Next, we’ll provide values, once again making sure that the number of sublists matches the number of labels:

[[17, 9, 13],[12,11,15],[15,8,17]];

Now when you click Run, the Bar Chart node will create a chart with three groups of bars, marked Index 0, 1, and 2, respectively. In this example, consider each index (i.e., group) a design variation. The values in the first group (Index 0) are pulled from the first item in each list in the values input, so the first group contains 17 for Model A, 12 for Model B, and 15 for Model C. The second group (Index 1) uses the second value in each group, and so on.

___
## Example File

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

