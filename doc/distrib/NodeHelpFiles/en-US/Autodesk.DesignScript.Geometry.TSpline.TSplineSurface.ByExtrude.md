## In-Depth
In the example below, a T-Spline surface is created as an extrusion of a given profile `curve`. The curve can be open or closed. The extrusion is performed in a provided `direction` and can be in both directions, controlled by inputs `frontDistance` and `backDistance`. The spans can be individually set for the two directions of extrusion, with the given `frontSpans` and `backSpans`. To establish the definition of the surface along the curve, `profileSpans` controls the number of faces and `uniform` either distributes them in a uniform fashion or takes curvature into account. Finally, `inSmoothMode` controls if the surface is displayed in a smooth or box mode.

## Example File
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)   
