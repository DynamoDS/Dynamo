## In-Depth
Un UVNFrame di una faccia fornisce informazioni utili sulla posizione e sull'orientamento della faccia restituendo le direzioni del vettore normale e UV.
Nell'esempio seguente, viene utilizzato un nodo `TSplineFace.UVNFrame` per visualizzare la distribuzione delle facce su una primitiva quadball. `TSplineTopology.DecomposedFaces` viene utilizzato per eseguire query su tutte le facce e viene quindi utilizzato un nodo `TSplineFace.UVNFrame` per recuperare le posizioni dei baricentri delle facce come punti. I punti vengono visualizzati utilizzando un nodo `TSplineUVNFrame.Position`. Le etichette vengono visualizzate nell'anteprima dello sfondo attivando Mostra etichette nel menu contestuale del nodo.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
