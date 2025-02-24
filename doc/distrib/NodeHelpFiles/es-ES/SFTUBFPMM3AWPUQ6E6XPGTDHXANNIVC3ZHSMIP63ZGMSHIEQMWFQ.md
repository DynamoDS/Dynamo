<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
`TSplineSurface.ByPlaneThreePoints` genera una superficie de plano de primitiva de T-Spline mediante tres puntos como entrada. Para crear el plano de T-Spline, el nodo utiliza las entradas siguientes:
- `p1`, `p2` y `p3`: tres puntos que definen la posición del plano. El primer punto se considera el origen del plano.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

En el ejemplo siguiente, se crea una superficie plana de T-Spline mediante tres puntos generados aleatoriamente. El primer punto es el origen del plano. El tamaño de la superficie se controla mediante los dos puntos utilizados como entradas `minCorner` y `maxCorner`.

## Archivo de ejemplo

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
