## In-Depth
`TSplineSurface.CreaseEdges` aggiunge una triangolazione netta al bordo specificato su una superficie T-Spline.
Nell'esempio seguente, una superficie T-Spline viene generata da un toro T-Spline. Viene selezionato un bordo utilizzando il nodo `TSplineTopology.EdgeByIndex` e a tale bordo viene applicata una triangolazione con l'aiuto del nodo `TSplineSurface.CreaseEdges`. Anche i vertici su entrambi i bordi vengono sottoposti a triangolazione. La posizione del bordo selezionato viene visualizzata in anteprima con l'aiuto dei nodi `TSplineEdge.UVNFrame` e `TSplineUVNFrame.Poision`.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
