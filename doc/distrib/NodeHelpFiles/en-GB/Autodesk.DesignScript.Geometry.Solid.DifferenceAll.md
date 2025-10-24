## In Depth
Difference All will create a new solid by subtracting a list of solids from one single solid. The 'solid' input indicates the solid to subtract from, while the 'tools' input is the list of solids that will be subtracted. The solids in this list will be unioned together to create a single solid, which is then subtracted from the 'solid' input. In the example below, we start with a default cube as the solid we are going to subtract from. We use a series of number sliders to control the position and radius of a sphere. By using a sequence of numbers as the z-coordinate, we create a list of several spheres. If the spheres are intersecting the cube, then the result is a cube with the intersecting parts of the spheres subtracted from it.
___
## Example File

![DifferenceAll](./Autodesk.DesignScript.Geometry.Solid.DifferenceAll_img.jpg)

