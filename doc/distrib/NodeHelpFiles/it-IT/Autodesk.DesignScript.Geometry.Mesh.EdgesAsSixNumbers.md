## In profondità
`Mesh.EdgesAsSixNumbers` determina le coordinate X, Y e Z dei vertici che compongono ogni bordo univoco in una determinata mesh, ottenendo quindi sei numeri per bordo. Questo nodo può essere utilizzato per eseguire query o ricostruire la mesh o i relativi bordi.

Nell'esempio seguente, `Mesh.Cuboid` viene utilizzato per creare una mesh cuboide, che viene quindi utilizzata come input per il nodo `Mesh.EdgesAsSixNumbers` per recuperare l'elenco di bordi espressi come sei numeri. L'elenco è suddiviso in elenchi di 6 elementi utilizzando `List.Chop`, quindi `List.GetItemAtIndex` e `Point.ByCoordinates` vengono utilizzati per ricostruire gli elenchi di punti iniziali e finali di ciascun bordo. Infine, `List.ByStartPointEndPoint` viene utilizzato per ricostruire i bordi della mesh.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
