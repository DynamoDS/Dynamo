<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormalXAxis` genera una superficie del piano della primitiva T-Spline utilizzando un punto di origine, un vettore normale e una direzione del vettore dell'asse X del piano. Per creare il piano T-Spline, il nodo utilizza i seguenti input:
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
- `xAxis`: un vettore che definisce la direzione dell'asse X, consentendo un maggiore controllo sull'orientamento del piano creato.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Nell'esempio seguente, viene creata una superficie T-Spline piana utilizzando il punto di origine fornito e la normale che è un vettore dell'asse X. L'input `xAxis` è impostato sull'asse Z. La dimensione della superficie è controllata dai due punti utilizzati come input `minCorner` e `maxCorner`.

## File di esempio

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
