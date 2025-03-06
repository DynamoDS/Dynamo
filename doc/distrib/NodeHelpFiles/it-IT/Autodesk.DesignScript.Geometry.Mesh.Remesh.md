## In profondità
`Mesh.Remesh` crea una nuova mesh in cui i triangoli in un determinato oggetto vengono ridistribuiti in modo più uniforme indipendentemente da qualsiasi modifica nelle normali dei triangoli. Questa operazione può risultare utile per mesh con densità variabile di triangoli al fine di preparare la mesh per l'analisi della resistenza. La rigenerazione ripetuta della mesh genera mesh progressivamente più uniformi. Per le mesh i cui vertici sono già equidistanti (ad esempio, una mesh icosfera), il risultato del nodo `Mesh.Remesh` è la stessa mesh.
Nell'esempio seguente, `Mesh.Remesh` viene utilizzato su una mesh importata con un'alta densità di triangoli in aree con dettagli precisi. Il risultato del nodo `Mesh.Remesh` viene traslato sul lato e `Mesh.Edges` viene utilizzato per visualizzare tale risultato.

`(The example file used is licensed under creative commons)`

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
