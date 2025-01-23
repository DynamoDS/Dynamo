<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
Mit `TSplineSurface.ByPlaneThreePoints` wird eine T-Spline-Grundkörper-Ebenenoberfläche erstellt, wobei drei Punkte als Eingabe verwendet werden. Zum Erstellen der T-Spline-Ebene verwendet der Block die folgenden Eingaben:
- `p1`, `p2` und `p3`: Drei Punkte, die die Position der Ebene definieren. Der erste Punkt wird als Ursprung der Ebene betrachtet.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Im folgenden Beispiel wird eine planare T-Spline-Oberfläche durch drei zufällig generierte Punkte erstellt. Der erste Punkt ist der Ursprung der Ebene. Die Größe der Oberfläche wird durch die beiden als `minCorner`- und `maxCorner`-Eingaben verwendeten Punkte gesteuert.

## Beispieldatei

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
