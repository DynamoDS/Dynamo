## In-Depth
`TSplineSurface.ByPlaneThreePoints` generates a T-Spline primitive plane surface using three points as input. To create the T-Spline Plane, the node uses the following inputs:
- `p1`, `p2` and `p3`: three points defining the position of the plane. The first point is considered the origin of the plane.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively. 
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

In the example below, a T-Spline planar surface is created by three randomly generated points. The first point is the origin of the plane. The size of the surface is controlled by the two points used as `minCorner` and `maxCorner` inputs. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints_img.jpg)