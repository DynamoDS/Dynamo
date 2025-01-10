## In Depth
Pull Onto Surface will create a new curve by projecting an input curve onto an input surface, using the normal vectors of the surface as the directions of projection. In the example below, we first create surface by using a Surface.BySweep node that uses curves generated according to a sine curve. This surface is used as the base surface to pull onto in a PullOntoSurface node. For the curve, we create a circle by using a Code Block to specify the coordinates of the center point, and a number slider to control the radius of the circle. The result is a projection of the cirle onto the surface. 
___
## Example File

![PullOntoSurface](./Autodesk.DesignScript.Geometry.Curve.PullOntoSurface_img.jpg)

