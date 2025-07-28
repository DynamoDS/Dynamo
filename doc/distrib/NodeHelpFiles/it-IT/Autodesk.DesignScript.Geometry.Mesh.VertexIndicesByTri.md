## In profondità
`Mesh.VertexIndicesByTri` restituisce un elenco non nidificato di indici dei vertici corrispondenti a ciascun triangolo della mesh. Gli indici sono ordinati in tre e i raggruppamenti di indici possono essere facilmente ricostruiti utilizzando il nodo `List.Chop` con l'input `lengths` pari a 3.

Nell'esempio seguente, `MeshToolkit.Mesh` con 20 triangoli viene convertito in `Geometry.Mesh`. `Mesh.VertexIndicesByTri` viene utilizzato per ottenere l'elenco di indici che viene poi diviso in elenchi di tre utilizzando `List.Chop`. La struttura dell'elenco viene capovolta utilizzando `List.Transpose` per ottenere tre elenchi di primo livello di 20 indici corrispondenti ai punti A, B e C in ogni triangolo della mesh. Il nodo `IndexGroup.ByIndices` viene utilizzato per creare gruppi di tre indici ciascuno. L'elenco strutturato di `IndexGroups` e l'elenco di vertici vengono quindi utilizzati come input per `Mesh.ByPointsFaceIndices` per ottenere una mesh convertita.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
