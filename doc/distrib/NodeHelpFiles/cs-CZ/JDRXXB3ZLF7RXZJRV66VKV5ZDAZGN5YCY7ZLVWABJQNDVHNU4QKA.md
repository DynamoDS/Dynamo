<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
Uzel `TSplineSurface.ByPlaneOriginXAxisYAxis` vygeneruje základní rovinu T-Spline pomocí bodu počátku a dvou vektorů představujících osy X a Y roviny. K vytvoření roviny T-Spline používá uzel následující vstupy:
- `origin`: a point defining the origin of the plane.
- `xAxis` a `yAxis`: vektory definující směr os X a Y vytvořené roviny.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

V níže uvedeném příkladu se vytvoří rovinný povrch T-Spline pomocí zadaného bodu počátku a dvou vektorů, které slouží jako směry X a Y. Velikost povrchu je řízena dvěma body, které se použijí jako vstupy 'minCorner` a `maxCorner`.

## Vzorový soubor

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
