## In Depth
Curve by IsoCurve on Surface will create a curve that is the isocurve on a surface by specifying the U or V direction, and specifying the parameter in the opposite direction at which to create the curve. The 'direction' input determines which direction of isocurve to create. A value of one corresponds to the u-direction, while a value of zero corresponds to the v-direction. In the example below, we first create grid of points, and translate them in the Z-direction by a random amount. These points are used to create surface by using a NurbsSurface.ByPoints node. This surface is used as the baseSurface of a ByIsoCurveOnSurface node. A number slider set to a range of 0 to 1 and a step of 1 is used to control whether we extract the isocurve in the u or in the v direction. A second number slider is used to determine the parameter at which the isocurve is extracted.
___
## Example File

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

