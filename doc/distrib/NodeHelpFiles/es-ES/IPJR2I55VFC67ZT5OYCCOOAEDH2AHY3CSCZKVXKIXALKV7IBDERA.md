<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormalXAxis` genera una superficie de plano de primitiva de T-Spline mediante un punto de origen, un vector normal y una dirección de vector del eje X del plano. Para crear el plano de T-Spline, el nodo utiliza las siguientes entradas:
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
- `xAxis`: un vector que define la dirección del eje X, lo que permite un mayor control de la orientación del plano creado.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

En el ejemplo siguiente, se crea una superficie plana de T-Spline mediante el punto de origen especificado y la normal, que es un vector del eje X. La entrada `xAxis` se establece en el eje Z. El tamaño de la superficie se controla mediante los dos puntos utilizados como entradas `minCorner` y `maxCorner`.

## Archivo de ejemplo

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
