## In-Depth
`TSplineEdge.IsBorder` restituisce `True` se il bordo T-Spline di input è un bordo.

Nell'esempio seguente, vengono esaminati i bordi di due superfici T-Spline. Le superfici sono un cilindro e la relativa versione ispessita. Per selezionare tutti i bordi, vengono utilizzati i nodi `TSplineTopology.EdgeByIndex` in entrambi i casi, con l'input indices, un intervallo di numeri interi compreso tra 0 e n, dove n è il numero di bordi fornito da `TSplineTopology.EdgesCount`. Si tratta di un'alternativa alla selezione diretta dei bordi utilizzando `TSplineTopology.DecomposedEdges`. `TSplineSurface.CompressIndices` viene inoltre utilizzato nel caso di un cilindro ispessito per riordinare gli indici dei bordi.
Viene utilizzato un nodo `TSplineEdge.IsBorder` per verificare quali sono i bordi. La posizione dei bordi del cilindro piatto viene evidenziata con l'aiuto dei nodi `TSplineEdge.UVNFrame` e `TSplineUVNFrame.Position`. Il cilindro ispessito non presenta bordi.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
