<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
Uzel `TSplineSurface.ByPlaneOriginNormalXAxis` vygeneruje základní rovinu povrchu T-Spline pomocí bodu počátku, normálového vektoru a směru vektoru osy X roviny. K vytvoření roviny T-spline použije uzel následující vstupy:
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
- `xAxis`: vektor definující směr osy X, který umožňuje větší kontrolu nad orientací vytvořené roviny.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

V níže uvedeném příkladu je vytvořen rovinný povrch T-Spline pomocí zadaného bodu počátku a normály, která je vektorem osy X. Vstup `xAxis' je nastaven na osu Z. Velikost povrchu je řízena dvěma body použitými jako vstupy `minCorner` a `maxCorner`.

## Vzorový soubor

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
