## In-Depth
Węzeł `TSplineSurface.CreaseEdges` dodaje ostre fałdowanie do określonej krawędzi na powierzchni T-splajn.
W poniższym przykładzie generowana jest powierzchnia T-splajn na podstawie torusa T-splajn. Za pomocą węzła `TSplineTopology.EdgeByIndex` zostaje wybrana krawędź, do której zostaje zastosowane fałdowanie za pomocą węzła `TSplineSurface.CreaseEdges`. Wierzchołki na obu krańcach tej krawędzi również są fałdowane. Podgląd położenia wybranej krawędzi jest wyświetlany za pomocą węzłów `TSplineEdge.UVNFrame` i `TSplineUVNFrame.Poision`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
