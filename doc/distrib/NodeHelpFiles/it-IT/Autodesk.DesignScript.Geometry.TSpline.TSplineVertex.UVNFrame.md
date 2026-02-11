## In-Depth
Questo nodo restituisce un oggetto TSplineUVNFrame che può essere utile per visualizzare la posizione e l'orientamento del vertice, nonché per utilizzare i vettori U, V o N per manipolare ulteriormente la superficie T-Spline.

Nell'esempio seguente, il nodo `TSplineVertex.UVNFrame` viene utilizzato per ottenere il Frame UVN del vertice selezionato. Il Frame UVN viene quindi utilizzato per restituire la normale del vertice. Infine, viene utilizzata la direzione normale per spostare il vertice utilizzando il nodo `TSplineSurface.MoveVertices`.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
