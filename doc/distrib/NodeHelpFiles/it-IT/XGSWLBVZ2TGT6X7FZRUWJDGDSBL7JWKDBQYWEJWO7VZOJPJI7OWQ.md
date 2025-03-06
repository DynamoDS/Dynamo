## In profondità
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` consente di modificare le posizioni dei punti di controllo per un determinato gruppo di vertici allineandoli con `parallelPlane` fornito come input.

Nell'esempio seguente, i vertici di una superficie del piano T-Spline vengono spostati utilizzando i nodi `TsplineTopology.VertexByIndex` e `TSplineSurface.MoveVertices`. La superficie viene quindi traslata sul lato per una migliore anteprima e utilizzata come input per un nodo `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. Il risultato è una nuova superficie con i vertici selezionati che risultano piatti sul piano fornito.
___
## File di esempio

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
