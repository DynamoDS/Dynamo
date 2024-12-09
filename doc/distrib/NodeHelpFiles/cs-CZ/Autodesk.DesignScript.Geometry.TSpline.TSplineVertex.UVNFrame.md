## In-Depth
Tento uzel vrací objekt TSplineUVNFrame, který může být užitečný k vizualizaci pozice a orientace vrcholu a také k další manipulaci s povrchem T-Spline pomocí vektorů U, V nebo N.

V níže uvedeném příkladu se pomocí uzlu `TSplineVertex.UVNFrame` získá rámová konstrukce UVN vybraného vrcholu. Rámová konstrukce UVN se poté použije k vrácení normály vrcholu. Nakonec se k přesunu vrcholu pomocí uzlu `TSplineSurface.MoveVertices` použije směr normály.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
