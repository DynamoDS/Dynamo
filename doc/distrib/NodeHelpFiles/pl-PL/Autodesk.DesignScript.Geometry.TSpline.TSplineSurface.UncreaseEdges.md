## In-Depth
Ten węzeł, jako przeciwny do węzła `TSplineSurface.CreaseEdges`, usuwa fałdowanie określonej krawędzi na powierzchni T-splajn.
W poniższym przykładzie generowana jest powierzchnia T-splajn na podstawie torusa T-splajn. Za pomocą węzłów `TSplineTopology.EdgeByIndex` oraz `TSplineTopology.EdgesCount` zostają wybrane wszystkie krawędzie i zostaje do nich zastosowane fałdowanie za pomocą węzła `TSplineSurface.CreaseEdges`. Następnie zostaje wybrany podzestaw krawędzi z indeksami od 0 do 7 i zostaje do niego zastosowana operacja odwrotna — tym razem przy użyciu węzła `TSplineSurface.UncreaseEdges`. Podgląd położeń wybranych krawędzi jest wyświetlany za pomocą węzłów `TSplineEdge.UVNFrame` i `TSplineUVNFrame.Poision`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
