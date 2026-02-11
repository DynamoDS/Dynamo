## In profondità
`Mesh.Reduce` crea una nuova mesh con un numero ridotto di triangoli. L'input `triangleCount` definisce il numero di triangoli di destinazione della mesh di output. Tenere presente che `Mesh.Reduce` potrebbe alterare in modo significativo la forma della mesh in caso di un numero di triangoli di destinazione `triangleCount` particolarmente alto. Nell'esempio seguente, `Mesh.ImportFile` viene utilizzato per importare una mesh, che viene quindi ridotta dal nodo `Mesh.Reduce` e traslata in un'altra posizione per migliorare anteprima e confronto.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
