## In profondità
`Mesh.CloseCracks` chiude le fessure in una mesh rimuovendo i contorni interni da un oggetto mesh. I contorni interni possono sorgere naturalmente in seguito a operazioni di modellazione della mesh. I triangoli possono essere eliminati in questa operazione se vengono rimossi i bordi degenerati. Nell'esempio seguente, `Mesh.CloseCracks` viene utilizzato su una mesh importata. `Mesh.VertexNormals` viene utilizzato per visualizzare i vertici sovrapposti. Dopo che la mesh originale è passata attraverso Mesh.CloseCracks, il numero di bordi viene ridotto, il che è evidente anche confrontando il numero di bordi, utilizzando un nodo `Mesh.EdgeCount`.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
