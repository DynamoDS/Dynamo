## In-Depth
`TSplineVertex.IsStarPoint` returns whether a vertex is a star point. 

Star points exist when 3, 5, or more edges come together. They naturally occur in the Box or Quadball primitive and are most commonly created when extruding a T-Spline face, deleting a face, or performing Merge. Unlike regular and T-Point vertices, star points are not controlled by rectangular rows of control points. Star points make the area around them more difficult to control and can create distortion, so they should only be used where necessary. Poor locations for star point placement include sharper parts of the model like creased edges, parts where the curvature changes significantly, or on the edge of an open surface.

In the example below, `TSplineVertex.IsStarPoint` is used to query if the Vertex selected with `TSplineTopology.VertexByIndex` is a star point. 


## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)