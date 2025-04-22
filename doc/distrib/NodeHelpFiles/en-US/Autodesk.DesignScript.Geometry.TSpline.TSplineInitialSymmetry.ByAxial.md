## In-Depth
`TSplineInitialSymmetry.ByAxial` defines if the T-Spline geometry has symmetry along a chosen axis (x, y, z). Symmetry can occur on one, two, or all three axes. Once established at the creation of the T-Spline geometry, symmetry influences all subsequent operations and alterations. 

In the example below, `TSplineSurface.ByBoxCorners` is used to create a T-Spline surface. Among the inputs of this node, `TSplineInitialSymmetry.ByAxial` is used to define the initial symmetry in the surface. `TSplineTopology.RegularFaces` and `TSplineSurface.ExtrudeFaces` are then used to respectively select and extrude a face of the T-Spline surface. The extrude operation is then mirrored around the symmetry axes defined with the `TSplineInitialSymmetry.ByAxial` node.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)