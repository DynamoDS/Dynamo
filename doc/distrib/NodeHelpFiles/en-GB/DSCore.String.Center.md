## In Depth
Polygon Center finds the center of a given polygon by taking the average value of the corners. For concave polygons, it's possible that the center may actually lie outside the polygon. In the example below, we first generate a list of random angles and radii to use as inputs to Point By Cylindrical Coordinates. By sorting the angles first, we ensure that the resulting polygon will be connected in order of increasing angle, and therefore will not be self-intersecting. We can then use Center to take the average of the points and find the polygon center.
___
## Example File

![Center](./DSCore.String.Center_img.jpg)

