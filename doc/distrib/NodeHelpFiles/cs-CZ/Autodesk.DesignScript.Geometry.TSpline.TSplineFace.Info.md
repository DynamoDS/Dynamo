## In-Depth
Uzel `TSplineFace.Info` vrací následující vlastnosti plochy T-Spline:
- `uvnFrame`: bod na trupu, vektor U, vektor V a normálový vektor plochy T-Spline
- `index`: index plochy
- `valence`: počet vrcholů nebo hran, které tvoří plochu.
- `sides`: počet hran každé plochy T-Spline

V níže uvedeném příkladu se pomocí uzlu `TSplineSurface.ByBoxCorners`, respektive uzlu `TSplineTopology.RegularFaces` vytvoří povrch T-Spline a vyberou se jeho plochy. Pomocí uzlu `List.GetItemAtIndex` se vyberou konkrétní plochy povrchu T-Spline a pomocí uzlu `TSplineFace.Info` se zjistí jeho vlastnosti.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
