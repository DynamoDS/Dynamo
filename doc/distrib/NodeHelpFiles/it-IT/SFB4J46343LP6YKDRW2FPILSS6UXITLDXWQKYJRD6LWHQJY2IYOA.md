<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint --->
<!--- SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA --->
## In-Depth
`TSplineSurface.ByPlaneLineAndPoint` genera una superficie del piano della primitiva T-Spline da una linea e un punto. La superficie T-Spline risultante è un piano. Per creare il piano T-Spline, il nodo utilizza i seguenti input:
- `line` e `point`: un input necessario per definire l'orientamento e la posizione del piano.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Nell'esempio seguente, viene creata una superficie T-Spline piana utilizzando una linea e un piano come input. La dimensione della superficie è controllata dai due punti utilizzati come input `minCorner` e `maxCorner`.

## File di esempio

![Example](./SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA_img.jpg)
