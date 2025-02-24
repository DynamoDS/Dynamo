## In profondità
`Mesh.Cone` crea una mesh cono la cui base è centrata in un punto di origine di input, con un valore di input per il raggio di base e quello superiore, altezza e un numero di `divisions`. Il numero di `divisions` corrisponde al numero di vertici che vengono creati nella parte superiore e alla base del cono. Se il numero di `divisions` è pari a 0, in Dynamo viene utilizzato un valore di default. Il numero di divisioni lungo l'asse Z è sempre uguale a 5. L'input `cap` utilizza un valore `Boolean` per controllare se il cono è chiuso nella parte superiore.
Nell'esempio seguente, il nodo `Mesh.Cone` viene utilizzato per creare una mesh a forma di cono con 6 divisioni, quindi la base e la parte superiore del cono sono esagoni. Il nodo `Mesh.Triangles` viene utilizzato per visualizzare la distribuzione dei triangoli della mesh.


## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
