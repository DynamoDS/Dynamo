<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
Węzeł `TSplineSurface.ByPlaneOriginNormalXAxis` generuje powierzchnię płaszczyzny prymitywu T-splajn, używając punktu początkowego, wektora normalnego i kierunku wektora osi X płaszczyzny. Aby utworzyć płaszczyznę T-splajn, węzeł ten używa następujących danych wejściowych:
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
— `xAxis`: wektor definiujący kierunek osi X, zapewniający większą kontrolę nad orientacją tworzonej płaszczyzny.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

W poniższym przykładzie zostaje utworzona powierzchnia płaska T-splajn przy użyciu podanego punktu początkowego i wektora normalnego, który jest wektorem osi X. Jako pozycję danych wejściowych `xAxis` ustawiono oś Z. Rozmiarem powierzchni sterują dwa punkty określane za pomocą danych wejściowych `minCorner` i `maxCorner`.

## Plik przykładowy

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
