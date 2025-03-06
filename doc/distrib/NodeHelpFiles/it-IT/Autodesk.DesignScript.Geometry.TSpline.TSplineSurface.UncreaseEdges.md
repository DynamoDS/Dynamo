## In-Depth
A differenza del nodo `TSplineSurface.CreaseEdges`, questo nodo rimuove la triangolazione del bordo specificato su una superficie T-Spline.
Nell'esempio seguente, viene generata una superficie T-Spline da un toro T-Spline. Tutti i bordi vengono selezionati utilizzando i nodi `TSplineTopology.EdgeByIndex` e `TSplineTopology.EdgesCount` e la triangolazione viene applicata a tutti i bordi con l'aiuto del nodo `TSplineSurface.CreaseEdges`. Viene quindi selezionato un sottogruppo di bordi con indici da 0 a 7 e viene applicata l'operazione inversa, questa volta, utilizzando il nodo `TSplineSurface.UncreaseEdges`. La posizione dei bordi selezionati viene visualizzata in anteprima con l'aiuto dei nodi `TSplineEdge.UVNFrame` e `TSplineUVNFrame.Poision`.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
