## In profondità
`Mesh.TrainglesAsNineNumbers` determina le coordinate X, Y e Z dei vertici che compongono ogni triangolo in una determinata mesh, ottenendo nove numeri per triangolo. Questo nodo può essere utile per eseguire query, ricostruire o convertire la mesh originale.

Nell'esempio seguente, `File Path` e `Mesh.ImportFile` vengono utilizzati per importare una mesh. Quindi `Mesh.TrianglesAsNineNumbers` viene utilizzato per ottenere le coordinate dei vertici di ogni triangolo. Questo elenco viene quindi suddiviso in tre utilizzando `List.Chop` con l'input `lengths` impostato su 3. `List.GetItemAtIndex` viene quindi utilizzato per ottenere ogni coordinata X, Y e Z e ricostruire i vertici utilizzando `Point.ByCoordinates`. L'elenco di punti è ulteriormente suddiviso in tre (3 punti per ogni triangolo) e viene utilizzato come input per `Polygon.ByPoints`.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
