<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## In Depth
`TSplineSurface.BuildFromLines` provides a way to create a more complex T-Spline surface that can either be used as the final geometry or as a custom primitive that is closer to the desired shape than default primitives. The result can either be a closed or open surface and can have holes and/or creased edges.

The input of the node is a list of curves that represent a 'control cage' for the TSpline surface. Setting up the list of lines requires some preparation and must follow certain guidelines. 
- the lines must not overlap
- the border of the polygon must be closed and each line endpoint must meet at least another endpoint. Each line intersection must meet at a point. 
- a bigger density of polygons is required for areas with greater detail 
- quads are preferred to triangles and nGons because they are easier to control.

In the example below, two T-Spline surfaces are created to illustrate the use of this node. `maxFaceValence` is left at default value for both cases and `snappingTolerance` is adjusted to assure that lines within the tolerance value are treated as joining. For the shape on the left, `creaseOuterVertices` is set to False to keep two corner vertices sharp and not rounded. The shape on the left does not feature outer vertices and this input is left at default value. `inSmoothMode` is activated for both shapes for a smooth preview.

___
## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines_img.jpg)
