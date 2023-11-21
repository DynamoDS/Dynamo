## In-Depth
The functional valence of a vertex goes beyond a simple count of adjacent edges and takes into account the virtual grid lines that impact the blending of the vertex in the area around it. It provides a more nuanced understanding of how vertices and their edges influence the surface during deformation and refinement operations. 
When used on regular vertices and T-Points, `TSplineVertex.FunctionalValence` node returns the value of "4" which means that the surface is guided by splines in a shape of a grid. A functional valence of anything other than "4" means that the vertex is a star point and the blending around the vertex will be less smooth.

In the example below, the `TSplineVertex.FunctionalValence` is used on two T-Point vertices of a T-Spline plane surface. The `TSplineVertex.Valence` node returns the value of 3, while the Functional Valence of the selected vertices is 4, which is specific for T-Points. `TSplineVertex.UVNFrame` and `TSplineUVNFrame.Position` nodes are used to visualize the position of the vertices being analyzed. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence_img.jpg)