## In profondità
`Mesh.ByVerticesIndices` utilizza un elenco di `Points`, che rappresentano `vertices`dei triangoli della mesh, e un elenco di `indices`, che rappresentano il modo in cui la mesh viene unita insieme, e crea una nuova mesh. L'input `vertices` deve essere un elenco non nidificato di vertici univoci nella mesh. L'input `indices` deve essere un elenco non nidificato di numeri interi. Ogni gruppo di tre numeri interi indica un triangolo nella mesh. I numeri interi specificano l'indice del vertice nell'elenco di vertices. L'input indices deve essere indicizzato a 0, con il primo punto dell'elenco di vertices con l'indice 0.

Nell'esempio seguente, viene utilizzato un nodo `Mesh.ByVerticesIndices` per creare una mesh utilizzando un elenco di nove `vertices`e un elenco di 36 `indices`, specificando la combinazione di vertici per ciascuno dei 12 triangoli della mesh.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
