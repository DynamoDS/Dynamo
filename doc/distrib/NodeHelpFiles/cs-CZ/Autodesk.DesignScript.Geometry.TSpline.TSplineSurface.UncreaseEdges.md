## In-Depth
Na rozdíl od uzlu `TSplineSurface.CreaseEdges` tento uzel odebírá vyostření určené hrany na povrchu T-Spline.
V níže uvedeném příkladu je povrch T-Spline vygenerován z anuloidu T-Spline. Všechny hrany jsou vybrány pomocí uzlů `TSplineTopology.EdgeByIndex` a `TSplineTopology.EdgesCount` a vyostření se použije na všechny hrany pomocí uzlu `TSplineSurface.CreaseEdges`. Poté se vybere podmnožina hran s indexy 0 až 7 a použije se na ně opačná operaci – tentokrát pomocí uzlu `TSplineSurface.UncreaseEdges`. Pozice vybraných hran se zobrazí v náhledu pomocí uzlů `TSplineEdge.UVNFrame` a `TSplineUVNFrame.Poision`.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
