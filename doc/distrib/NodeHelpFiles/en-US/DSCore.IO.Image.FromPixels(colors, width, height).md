## In Depth
From Pixels with width and height will create an image from an input flat list of colors, where each color will become one pixel. The width multiplied by the height should equal the total number of colors. In the example below, we first create a list of colors using a ByARGB node. A code block creates a range of values from 0 to 255, which when connected to the r and g inputs produces a series of colors from black to yellow. We create a image with a width of 8. A Count node and Division node are used to determine the height of the image. A Watch Image node can be used to preview the image created.
___
## Example File

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

