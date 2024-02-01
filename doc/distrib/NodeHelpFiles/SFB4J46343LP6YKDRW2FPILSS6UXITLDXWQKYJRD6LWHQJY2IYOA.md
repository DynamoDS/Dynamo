<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint --->
<!--- SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA --->
## In-Depth
`TSplineSurface.ByPlaneLineAndPoint` generates a T-Spline primitive plane surface from a line and a point. The resulting T-Spline surface is a plane. To create the T-Spline Plane, the node uses the following inputs:
- `line` and `point`: an input required to define the orientation and position of the plane.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively. 
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

In the example below, a T-Spline planar surface is created using a line and a plane as input. The size of the surface is controlled by the two points used as `minCorner` and `maxCorner` inputs. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint_img.jpg)