## In profondità
`Mesh.ByPointsIndices` utilizza un elenco di `Points`, che rappresentano i `vertices` dei triangoli della mesh, e un elenco di `indices`, che rappresentano il modo in cui la mesh viene unita insieme, e crea una nuova mesh. L'input `points` deve essere un elenco semplice di vertici univoci nella mesh. L'input `indices` deve essere un elenco semplice di numeri interi. Ogni gruppo di tre numeri interi indica un triangolo nella mesh. I numeri interi specificano l'indice del vertice nell'elenco dei vertici. L'input indices deve essere indicizzato in base 0, dove il primo punto dell'elenco dei vertici ha l'indice 0.

Nell'esempio seguente, viene utilizzato un nodo `Mesh.ByPointsIndices` per creare una mesh utilizzando un elenco di nove `points` e un elenco di 36 `indices`, specificando la combinazione di vertici per ciascuno dei 12 triangoli della mesh.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
