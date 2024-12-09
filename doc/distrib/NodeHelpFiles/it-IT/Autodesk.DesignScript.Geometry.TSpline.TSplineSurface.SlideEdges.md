## In profondità
Nell'esempio seguente, viene creata una semplice superficie del parallelepipedo T-Spline e uno dei suoi bordi viene selezionato utilizzando il nodo `TSplineTopology.EdgeByIndex`. Per una migliore comprensione della posizione del vertice scelto, viene visualizzata con l'aiuto dei nodi `TSplineEdge.UVNFrame` e `TSplineUVNFrame.Position`. Il bordo scelto viene passato come input per il nodo `TSplineSurface.SlideEdges`, insieme alla superficie a cui appartiene. L'input `amount` determina la misura in cui il bordo scivola verso i bordi adiacenti, espressa in percentuale. L'input `roundness` controlla la planarità o la rotondità della smussatura. L'effetto della rotondità è compreso meglio in modalità riquadro. Il risultato dell'operazione di scorrimento viene quindi convertito sul lato per l'anteprima.

___
## File di esempio

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
