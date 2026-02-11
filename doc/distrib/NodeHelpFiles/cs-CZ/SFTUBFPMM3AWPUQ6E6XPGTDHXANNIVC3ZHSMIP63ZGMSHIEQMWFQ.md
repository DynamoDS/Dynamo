<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
Uzel `TSplineSurface.ByPlaneThreePoints` generuje základní rovinu T-Spline pomocí tří bodů jako vstupu. K vytvoření roviny T-Spline používá uzel následující vstupy:
- `p1`, `p2` a `p3`: tři body určující pozici roviny. První bod je považován za počátek roviny.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

V níže uvedeném příkladu je vytvořen rovinný povrch T-Spline pomocí třech náhodně vygenerovaných bodů. První bod je počátek roviny. Velikost povrchu je řízena dvěma body, které se použijí jako vstupy `minCorner` a `maxCorner`.

## Vzorový soubor

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
