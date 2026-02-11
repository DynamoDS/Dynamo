## In Depth
Extend With Arc will add a circular arc to the beginning or end of an input PolyCurve, and returns a single combined PolyCurve. The radius input will determin the radius of the circle, while the length input determines the distance along the circle for the arc. The total length must be less than or equal to length of a complete circle with the given radius. The generated arc will be tangent to the end of the input PolyCurve. A boolean input for endOrStart controls which end of the PolyCurve the arc will be created. A value of 'true' will result in the arc created at the end of the PolyCurve, while 'false' will create the arc at the beginning of the PolyCurve. In the example below, we first use a set of random points and PolyCurve By Points to generate a PolyCurve. We then use two number sliders and a boolean toggle to set the parametrs for ExtendWithArc.
___
## Example File

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

