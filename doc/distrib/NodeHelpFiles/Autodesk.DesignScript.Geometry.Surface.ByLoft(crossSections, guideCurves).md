## In Depth
`Surface.ByLoft (crossSections, guideCurves)` takes a list of cross sections to loft between, and a separate list of guide curves to determine the profile of the loft. 

In the example below, we use two straight lines as the input cross sections. For the guide curves, we create one sine curve and one straight line. A number slider controls the distance between the sine curve and the straight line. The resulting loft interpolates between the sine curve and the straight line.

___
## Example File

![Surface.ByLoft](./Autodesk.DesignScript.Geometry.Surface.ByLoft(crossSections,%20guideCurves)_img.jpg)