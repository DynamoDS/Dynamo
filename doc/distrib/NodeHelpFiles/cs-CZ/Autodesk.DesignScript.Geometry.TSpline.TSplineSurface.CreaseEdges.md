## In-Depth
Uzel `TSplineSurface.CreaseEdges` přidá k určené hraně na povrchu T-Spline ostrý přehyb.
V níže uvedeném příkladu je povrch T-spline vytvořen z anuloidu T-Spline. Hrana je vybrána pomocí uzlu `TSplineTopology.EdgeByIndex` a vyostření se použije na tuto hranu pomocí uzlu `TSplineSurface.CreaseEdges`. Vrcholy na obou hranách hrany jsou také vyostřené. Pozice vybrané hrany je zobrazena v náhledu pomocí uzlů `TSplineEdge.UVNFrame` a `TSplineUVNFrame.Poision`.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
